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
        private const string Program_InspectPopup_Flag_Color = "3DB9D3";
        ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
        private const string GameplanIntegrationService = "Gameplan Integration Service";
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

            var lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList();
            if (lstAllowedBusinessUnits.Count > 0)
                lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));
            if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin) && lstAllowedBusinessUnits.Count == 0) //else if (Sessions.IsDirector || Sessions.IsClientAdmin)
            {
                var clientBusinessUnit = db.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId) && b.IsDeleted == false).Select(b => b.BusinessUnitId).ToList<Guid>();
                businessUnitIds = clientBusinessUnit.ToList();
                var lstBusinessUnitIds = Common.GetBussinessUnitIds(Sessions.User.ClientId); //commented due to not used any where
                lstBusinessUnitIds = lstBusinessUnitIds.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
                planmodel.BusinessUnitIds = lstBusinessUnitIds;
                ViewBag.BusinessUnitIds = planmodel.BusinessUnitIds;//Added by Nirav for Custom Dropdown - 388
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
                    lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));
                    var lstClientBusinessUnits = Common.GetBussinessUnitIds(Sessions.User.ClientId);
                    lstClientBusinessUnits = lstClientBusinessUnits.Where(a => businessUnitIds.Contains(Guid.Parse(a.Value))).ToList().Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
                    planmodel.BusinessUnitIds = lstClientBusinessUnits;
                    ViewBag.BusinessUnitIds = lstClientBusinessUnits;
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
                        if (planmodel.BusinessUnitIds.Count > 0)
                        {
                            ViewBag.BusinessUnitTitle = planmodel.BusinessUnitIds.Where(b => b.Value.ToLower() == currentPlan.Model.BusinessUnitId.ToString().ToLower()).Select(b => b.Text).FirstOrDefault();
                        }
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
                ViewBag.IsBusinessUnitEditable = Common.IsBusinessUnitEditable(Sessions.BusinessUnitId);    // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                return View(planmodel);
            }
            else
            {
                TempData["ErrorMessage"] = Common.objCached.NoPublishPlanAvailable;  //// Error Message modified by Sohel Pathan on 22/05/2014 to address internal review points
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
            var lstSubordinates = Common.GetSubOrdinatesWithPeersNLevel();
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
            objHomePlan.plans = planList;

            List<ViewByModel> lstViewByTab = Common.GetDefaultGanttTypes(null, Convert.ToString(currentPlanId));
            lstViewByTab = lstViewByTab.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewByTab = lstViewByTab;

            List<SelectListItem> lstUpComingActivity = UpComingActivity(Convert.ToString(currentPlanId));
            lstUpComingActivity = lstUpComingActivity.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewUpComingActivity = lstUpComingActivity;

            return PartialView("_PlanDropdown", objHomePlan);
        }

        #endregion

        #region "UserPhoto"
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
        public JsonResult GetViewControlDetail(string viewBy, string planId, string isQuarter, string businessunitIds, string geographyIds, string verticalIds, string audienceIds, string ownerIds, string activeMenu)
        {
            #region "For all users"

            //// BusinessUnit Filter Criteria.
            List<Guid> filteredBusinessUnitIds = string.IsNullOrWhiteSpace(businessunitIds) ? new List<Guid>() : businessunitIds.Split(',').Select(bu => Guid.Parse(bu)).ToList();

            //// Create plan list based on PlanIds and Businessunit Ids filter criteria
            List<int> planIds = string.IsNullOrWhiteSpace(planId) ? new List<int>() : planId.Split(',').Select(plan => int.Parse(plan)).ToList();
            var plans = db.Plans.Where(p => planIds.Contains(p.PlanId) && p.IsDeleted.Equals(false) && filteredBusinessUnitIds.Contains(p.Model.BusinessUnitId)).Select(p => p).ToList();

            //// Custom Restriction for BusinessUnit
            var lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList();
            if (lstAllowedBusinessUnits.Count > 0)
            {
                List<Guid> businessUnitIds = new List<Guid>();
                lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));
                plans = plans.Where(p => businessUnitIds.Contains(p.Model.BusinessUnitId)).Select(p => p).ToList();
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

            //string planYear = plans.Select(a => a.Year).FirstOrDefault();
            CalendarStartDate = CalendarEndDate = DateTime.Now;
            Common.GetPlanGanttStartEndDate(planYear, isQuarter, ref CalendarStartDate, ref CalendarEndDate);

            //Start Maninder Singh Wadhva : 11/15/2013 - Getting list of tactic for view control for plan version id.
            //// Selecting campaign(s) of plan whose IsDelete=false.
            var campaign = db.Plan_Campaign.Where(pc => filteredPlanIds.Contains(pc.PlanId) && pc.IsDeleted.Equals(false))
                                           .Select(pc => pc).ToList()
                                           .Where(pc => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate, CalendarEndDate, pc.StartDate, pc.EndDate).Equals(false));

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
            var lstUserCustomRestriction = Common.GetUserCustomRestriction();
            int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            var lstAllowedVertical = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId).ToList();
            var lstAllowedGeography = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId.ToString().ToLower()).ToList();////Modified by Mitesh Vaishnav For functional review point 89
            tactic = tactic.Where(t => lstAllowedVertical.Contains(t.VerticalId.ToString()) && lstAllowedGeography.Contains(t.GeographyId.ToString().ToLower())).ToList();////Modified by Mitesh Vaishnav For functional review point 89

            var lstSubordinatesWithPeers = Common.GetSubOrdinatesWithPeersNLevel();
            var subordinatesTactic = tactic.Where(t => lstSubordinatesWithPeers.Contains(t.CreatedBy)).ToList();
            var subordinatesImprovementTactic = improvementTactic.Where(t => lstSubordinatesWithPeers.Contains(t.CreatedBy)).ToList();

            string tacticStatus = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            //// Modified By Maninder Singh Wadhva PL Ticket#47, Modofied by Dharmraj #538
            string requestCount = Convert.ToString(subordinatesTactic.Where(t => t.Status.Equals(tacticStatus)).Count() + subordinatesImprovementTactic.Where(improveTactic => improveTactic.Status.Equals(tacticStatus)).Count());

            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, activeMenu.ToLower());

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

            if (viewBy.Equals(PlanGanttTypes.Tactic.ToString(), StringComparison.OrdinalIgnoreCase) || viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return PrepareTacticAndRequestTabResult(campaign.ToList(), program.ToList(), tactic.ToList(), improvementTactic, requestCount, planYear, improvementTacticForAccordion, improvementTacticTypeForAccordion, planId);
            }
            else
            {
                return PrepareCustomFieldResult(viewBy, campaign.ToList(), program.ToList(), tactic.ToList(), improvementTactic, requestCount, planYear, improvementTacticForAccordion, improvementTacticTypeForAccordion, planId);
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
        private JsonResult PrepareCustomFieldResult(string viewBy, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion, string planId)
        {
            int CustomTypeId = 0;
            if (viewBy.Contains(Common.CustomTitle))
            {
                CustomTypeId = Convert.ToInt32(viewBy.Replace(Common.CustomTitle, ""));
                viewBy = PlanGanttTypes.Custom.ToString();
            }

            List<TacticTaskList> lstTacticTaskList = new List<TacticTaskList>();
            List<CustomFields> lstCustomFields = new List<CustomFields>();
            List<ImprovementTaskDetail> lstImprovementTaskDetails = new List<ImprovementTaskDetail>();

            if (viewBy.Equals(PlanGanttTypes.Vertical.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in tactic)
                {
                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = item,
                        CustomFieldId = item.VerticalId.ToString(),
                        CustomFieldTitle = item.Vertical.Title,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", item.VerticalId, item.Plan_Campaign_Program.Plan_Campaign.PlanId, item.Plan_Campaign_Program.PlanCampaignId, item.PlanProgramId, item.PlanTacticId),
                        ColorCode = item.Vertical.ColorCode,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForCustomField(PlanGanttTypes.Vertical, item.VerticalId, campaign, program, tactic)),
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                                    GetMinStartDateForCustomField(PlanGanttTypes.Vertical, item.VerticalId, campaign, program, tactic),
                                                                    GetMaxEndDateForCustomField(PlanGanttTypes.Vertical, item.VerticalId, campaign, program, tactic)),
                        CampaignProgress = GetCampaignProgress(tactic.Where(t => t.VerticalId == item.VerticalId).Select(t => t).ToList(),
                                                                item.Plan_Campaign_Program.Plan_Campaign,
                                                                improvementTactic,
                                                                item.Plan_Campaign_Program.Plan_Campaign.PlanId),
                        ProgramProgress = GetProgramProgress(tactic.Where(t => t.VerticalId == item.VerticalId).Select(t => t).ToList(),
                                                                item.Plan_Campaign_Program,
                                                                improvementTactic,
                                                                item.Plan_Campaign_Program.Plan_Campaign.PlanId),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlanNew(viewBy,
                                                    item.VerticalId, item.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                    tactic.Where(t => t.VerticalId == item.VerticalId).Select(t => t).ToList())),
                                                 Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                    CalendarEndDate,
                                                    GetMinStartDateForPlanNew(viewBy, item.VerticalId, item.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                    tactic.Where(t => t.VerticalId == item.VerticalId).Select(t => t).ToList()),
                                                    GetMaxEndDateForPlanNew(viewBy, item.VerticalId,
                                                    item.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                    tactic.Where(t => t.VerticalId == item.VerticalId).Select(t => t).ToList())),
                                                tactic.Where(t => t.VerticalId == item.VerticalId).Select(t => t).ToList(), improvementTactic, item.Plan_Campaign_Program.Plan_Campaign.PlanId),
                    });
                }

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
                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = item,
                        CustomFieldId = item.StageId.ToString(),
                        CustomFieldTitle = item.Stage.Title,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", item.StageId, item.Plan_Campaign_Program.Plan_Campaign.PlanId, item.Plan_Campaign_Program.PlanCampaignId, item.PlanProgramId, item.PlanTacticId),
                        ColorCode = item.Stage.ColorCode,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(item.StageId)).ToList<Plan_Campaign_Program_Tactic>())),
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                  CalendarEndDate,
                                                                  GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(item.StageId)).ToList<Plan_Campaign_Program_Tactic>()),
                                                                  GetMaxEndDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(item.StageId)).ToList<Plan_Campaign_Program_Tactic>())),
                        CampaignProgress = GetCampaignProgress(tactic.Where(t => t.StageId == item.StageId).Select(t => t).ToList(),
                                                                item.Plan_Campaign_Program.Plan_Campaign,
                                                                improvementTactic,
                                                                item.Plan_Campaign_Program.Plan_Campaign.PlanId),
                        ProgramProgress = GetProgramProgress(tactic.Where(t => t.StageId == item.StageId).Select(t => t).ToList(),
                                                                item.Plan_Campaign_Program,
                                                                improvementTactic,
                                                                item.Plan_Campaign_Program.Plan_Campaign.PlanId),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlanNew(viewBy,
                                                                item.StageId, item.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                                tactic.Where(t => t.StageId == item.StageId).Select(t => t).ToList())),
                                                             Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                CalendarEndDate,
                                                                GetMinStartDateForPlanNew(viewBy, item.StageId, item.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                                tactic.Where(t => t.StageId == item.StageId).Select(t => t).ToList()),
                                                                GetMaxEndDateForPlanNew(viewBy, item.StageId,
                                                                item.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                                tactic.Where(t => t.StageId == item.StageId).Select(t => t).ToList())),
                                                            tactic.Where(t => t.StageId == item.StageId).Select(t => t).ToList(), improvementTactic, item.Plan_Campaign_Program.Plan_Campaign.PlanId),
                    });
                }

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
                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = item,
                        CustomFieldId = item.AudienceId.ToString(),
                        CustomFieldTitle = item.Audience.Title,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", item.AudienceId, item.Plan_Campaign_Program.Plan_Campaign.PlanId, item.Plan_Campaign_Program.PlanCampaignId, item.PlanProgramId, item.PlanTacticId),
                        ColorCode = item.Audience.ColorCode,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForCustomField(PlanGanttTypes.Audience, item.AudienceId, campaign, program, tactic)),
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          GetMinStartDateForCustomField(PlanGanttTypes.Audience, item.AudienceId, campaign, program, tactic),
                                                          GetMaxEndDateForCustomField(PlanGanttTypes.Audience, item.AudienceId, campaign, program, tactic)),
                        CampaignProgress = GetCampaignProgress(tactic.Where(t => t.AudienceId == item.AudienceId).Select(t => t).ToList(),
                                                                item.Plan_Campaign_Program.Plan_Campaign,
                                                                improvementTactic,
                                                                item.Plan_Campaign_Program.Plan_Campaign.PlanId),
                        ProgramProgress = GetProgramProgress(tactic.Where(t => t.AudienceId == item.AudienceId).Select(t => t).ToList(),
                                                                item.Plan_Campaign_Program,
                                                                improvementTactic,
                                                                item.Plan_Campaign_Program.Plan_Campaign.PlanId),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlanNew(viewBy,
                                                                item.AudienceId, item.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                                tactic.Where(t => t.AudienceId == item.AudienceId).Select(t => t).ToList())),
                                                             Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                CalendarEndDate,
                                                                GetMinStartDateForPlanNew(viewBy, item.AudienceId, item.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                                tactic.Where(t => t.AudienceId == item.AudienceId).Select(t => t).ToList()),
                                                                GetMaxEndDateForPlanNew(viewBy, item.AudienceId,
                                                                item.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                                tactic.Where(t => t.AudienceId == item.AudienceId).Select(t => t).ToList())),
                                                            tactic.Where(t => t.AudienceId == item.AudienceId).Select(t => t).ToList(), improvementTactic, item.Plan_Campaign_Program.Plan_Campaign.PlanId),
                    });
                }

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
                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = item,
                        CustomFieldId = item.BusinessUnitId.ToString(),
                        CustomFieldTitle = item.BusinessUnit.Title,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", item.BusinessUnitId, item.Plan_Campaign_Program.Plan_Campaign.PlanId, item.Plan_Campaign_Program.PlanCampaignId, item.PlanProgramId, item.PlanTacticId),
                        ColorCode = item.BusinessUnit.ColorCode,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(pt => pt.BusinessUnitId.Equals(item.BusinessUnitId)).Select(pt => pt).ToList())),
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, GetMinStartDateStageAndBusinessUnit(campaign, program,
                                                            tactic.Where(pt => pt.BusinessUnitId.Equals(item.BusinessUnitId)).Select(pt => pt).ToList()),
                                                            GetMaxEndDateStageAndBusinessUnit(campaign, program,
                                                                tactic.Where(pt => pt.BusinessUnitId.Equals(item.BusinessUnitId)).Select(pt => pt).ToList())),
                        CampaignProgress = GetCampaignProgress(tactic.Where(t => t.BusinessUnitId == item.BusinessUnitId).Select(t => t).ToList(),
                                                                item.Plan_Campaign_Program.Plan_Campaign,
                                                                improvementTactic,
                                                                item.Plan_Campaign_Program.Plan_Campaign.PlanId),
                        ProgramProgress = GetProgramProgress(tactic.Where(t => t.BusinessUnitId == item.BusinessUnitId).Select(t => t).ToList(),
                                                                item.Plan_Campaign_Program,
                                                                improvementTactic,
                                                                item.Plan_Campaign_Program.Plan_Campaign.PlanId),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlanNew(viewBy,
                                                                item.PlanTacticId, item.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                                tactic.Where(t => t.BusinessUnitId == item.BusinessUnitId).Select(t => t).ToList())),
                                                             Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                CalendarEndDate,
                                                                GetMinStartDateForPlanNew(viewBy, item.PlanTacticId, item.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                                tactic.Where(t => t.BusinessUnitId == item.BusinessUnitId).Select(t => t).ToList()),
                                                                GetMaxEndDateForPlanNew(viewBy, item.PlanTacticId,
                                                                item.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                                tactic.Where(t => t.BusinessUnitId == item.BusinessUnitId).Select(t => t).ToList())),
                                                            tactic.Where(t => t.BusinessUnitId == item.BusinessUnitId).Select(t => t).ToList(), improvementTactic, item.Plan_Campaign_Program.Plan_Campaign.PlanId),
                    });
                }

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

                var tempTactic = (from cf in db.CustomFields
                                  join cft in db.CustomFieldTypes on cf.CustomFieldTypeId equals cft.CustomFieldTypeId
                                  join cfe in db.CustomField_Entity on cf.CustomFieldId equals cfe.CustomFieldId
                                  join t in db.Plan_Campaign_Program_Tactic on cfe.EntityId equals t.PlanTacticId
                                  join cfoLeft in db.CustomFieldOptions on new { Key1 = cf.CustomFieldId, Key2 = cfe.Value.Trim() } equals
                                     new { Key1 = cfoLeft.CustomFieldId, Key2 = SqlFunctions.StringConvert((double)cfoLeft.CustomFieldOptionId).Trim() } into cAll
                                  from cfo in cAll.DefaultIfEmpty()
                                  where cf.IsDeleted == false && t.IsDeleted == false && cf.EntityType == "Tactic" && cf.CustomFieldId == CustomTypeId &&
                                  cf.ClientId == Sessions.User.ClientId && tacticIdList.Contains(t.PlanTacticId)
                                  select new
                                  {
                                      tactic = t,
                                      masterCustomFieldId = cf.CustomFieldId,
                                      customFieldId = cft.Name == "DropDownList" ? (cfo.CustomFieldOptionId == null ? 0 : cfo.CustomFieldOptionId) : cfe.CustomFieldEntityId,
                                      customFieldTitle = cft.Name == "DropDownList" ? cfo.Value : cfe.Value,
                                  }).ToList().Distinct().ToList().ToList();

                var newtactic = tempTactic.Select(t => new
                {
                    tactic = t.tactic,
                    masterCustomFieldId = t.masterCustomFieldId,
                    customFieldId = tempTactic.Where(tt => tt.masterCustomFieldId == t.masterCustomFieldId && tt.customFieldTitle.Trim() == t.customFieldTitle.Trim()).Select(tt => tt.customFieldId).First(),// t.customFieldId,
                    customFieldTitle = t.customFieldTitle,
                }).ToList();

                foreach (var item in newtactic)
                {
                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = item.tactic,
                        CustomFieldId = item.customFieldId.ToString(),
                        CustomFieldTitle = item.customFieldTitle,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", item.customFieldId, item.tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, item.tactic.Plan_Campaign_Program.PlanCampaignId, item.tactic.PlanProgramId, item.tactic.PlanTacticId),
                        ColorCode = Common.ColorCodeForCustomField,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForCustomField(PlanGanttTypes.Custom, item.tactic.PlanTacticId, campaign, program, tactic)),
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForCustomField(PlanGanttTypes.Custom, item.tactic.PlanTacticId, campaign, program, tactic),
                                                          GetMaxEndDateForCustomField(PlanGanttTypes.Custom, item.tactic.PlanTacticId, campaign, program, tactic)),
                        CampaignProgress = GetCampaignProgress(tactic.Where(t => t.PlanTacticId == item.tactic.PlanTacticId).Select(t => t).ToList(),
                                                                item.tactic.Plan_Campaign_Program.Plan_Campaign,
                                                                improvementTactic,
                                                                item.tactic.Plan_Campaign_Program.Plan_Campaign.PlanId),
                        ProgramProgress = GetProgramProgress(tactic.Where(t => t.PlanTacticId == item.tactic.PlanTacticId).Select(t => t).ToList(),
                                                                item.tactic.Plan_Campaign_Program,
                                                                improvementTactic,
                                                                item.tactic.Plan_Campaign_Program.Plan_Campaign.PlanId),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlanNew(viewBy,
                                                                item.tactic.PlanTacticId, item.tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                                tactic.Where(t => t.PlanTacticId == item.tactic.PlanTacticId).Select(t => t).ToList())),
                                                             Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                CalendarEndDate,
                                                                GetMinStartDateForPlanNew(viewBy, item.tactic.PlanTacticId, item.tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                                tactic.Where(t => t.PlanTacticId == item.tactic.PlanTacticId).Select(t => t).ToList()),
                                                                GetMaxEndDateForPlanNew(viewBy, item.tactic.PlanTacticId,
                                                                item.tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program,
                                                                tactic.Where(t => t.PlanTacticId == item.tactic.PlanTacticId).Select(t => t).ToList()
                                                                )),
                                                            tactic.Where(t => t.PlanTacticId == item.tactic.PlanTacticId).Select(t => t).ToList()
                                                            , improvementTactic, item.tactic.Plan_Campaign_Program.Plan_Campaign.PlanId),

                    });
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

            #region Prepare tactic list for left side Accordian
            var lstCustomFieldTactics = lstTacticTaskList.Select(pcpt => new
            {
                PlanTacticId = pcpt.Tactic.PlanTacticId,
                CustomFieldId = pcpt.CustomFieldId,
                Title = pcpt.Tactic.Title,
                TaskId = pcpt.TaskId,
            }).ToList().Distinct().ToList();
            #endregion

            var lstTaskDetails = lstTacticTaskList.Select(t => new
            {
                MainParentId = t.CustomFieldId,
                MainParentTitle = t.CustomFieldTitle,
                StartDate = t.StartDate,
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
            }).ToList().Distinct().ToList();

            #region Custom Field
            var taskDataLevel1 = lstTaskDetails.Select(t => new
            {
                id = string.Format("Z{0}", t.MainParentId),
                text = t.MainParentTitle,
                start_date = t.StartDate,
                duration = t.Duration,
                progress = t.Progress,
                open = t.Open,
                color = t.Color
            }).Select(v => v).Distinct().OrderBy(t => t.text);

            var newTaskDataCustomField = taskDataLevel1.Select(v => new
            {
                id = v.id,
                text = v.text,
                start_date = v.start_date,
                duration = v.duration,
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
                (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase))) ? t.Tactic.PlanTacticId : Convert.ToInt32(t.MainParentId)), t.PlanId, campaign, program, tactic)),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          GetMinStartDateForPlanNew(viewBy,
                                                          ((viewBy.Equals(PlanGanttTypes.BusinessUnit.ToString(), StringComparison.OrdinalIgnoreCase) ||
                                                          (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)))
                                                          ? t.Tactic.PlanTacticId : Convert.ToInt32(t.MainParentId)),
                                                          t.PlanId, campaign, program, tactic),
                                                          GetMaxEndDateForPlanNew(viewBy,
                                                          ((viewBy.Equals(PlanGanttTypes.BusinessUnit.ToString(), StringComparison.OrdinalIgnoreCase) ||
                                                          (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)))
                                                          ? t.Tactic.PlanTacticId : Convert.ToInt32(t.MainParentId)),
                                                          t.PlanId, campaign, program, tactic)),
                progress = t.PlanProgress,  //GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlanNew(viewBy,
                //         ((viewBy.Equals(PlanGanttTypes.BusinessUnit.ToString(), StringComparison.OrdinalIgnoreCase) ||
                //         (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)))
                //         ? t.Tactic.PlanTacticId : Convert.ToInt32(t.MainParentId)), 
                //         t.PlanId, campaign, program, tactic)),
                //         Common.GetEndDateAsPerCalendar(CalendarStartDate,
                //           CalendarEndDate,
                //           GetMinStartDateForPlanNew(viewBy, ((viewBy.Equals(PlanGanttTypes.BusinessUnit.ToString(), StringComparison.OrdinalIgnoreCase) ||
                //           (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)))
                //           ? t.Tactic.PlanTacticId : Convert.ToInt32(t.MainParentId)), 
                //           t.PlanId, campaign, program, tactic),
                //           GetMaxEndDateForPlanNew(viewBy, ((viewBy.Equals(PlanGanttTypes.BusinessUnit.ToString(), StringComparison.OrdinalIgnoreCase) ||
                //           (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase))) ? t.Tactic.PlanTacticId : Convert.ToInt32(t.MainParentId)),
                //           t.PlanId, campaign, program, tactic)),
                //tactic, improvementTactic, t.PlanId),
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
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          ita.MinStartDate,
                                                          CalendarEndDate) - 1,
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
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          it.ImprovementTactic.EffectiveDate,
                                                          CalendarEndDate) - 1,
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

            List<ViewByModel> lstViewById = Common.GetDefaultGanttTypes(tactic.ToList().Select(t => t.PlanTacticId).ToList(), planId);
            lstViewById = lstViewById.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            return Json(new
            {
                customFieldTactics = lstCustomFieldTactics,
                customFields = lstCustomFields,
                taskData = finalTaskData,
                requestCount = requestCount,
                planYear = planYear,
                improvementTacticForAccordion = viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase) ? 0 : improvementTacticForAccordion,
                improvementTacticTypeForAccordion = viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase) ? 0 : improvementTacticTypeForAccordion,
                ViewById = lstViewById
            }, JsonRequestBehavior.AllowGet);
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
        private JsonResult PrepareTacticAndRequestTabResult(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion, string planId)
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
            List<object> tacticAndRequestTaskData = GetTaskDetailTactic(campaign.ToList(), program.ToList(), tactic.ToList(), improvementTactic);

            List<ViewByModel> lstViewById = Common.GetDefaultGanttTypes(tactic.ToList().Select(t => t.PlanTacticId).ToList(), planId);
            lstViewById = lstViewById.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
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
                ViewById = lstViewById
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Prepare Vertical Tab data
        ///// <summary>
        ///// Prepare Json result for Vertical tab to be rendered in gantt chart
        ///// </summary>
        ///// <param name="campaign">list of campaigns</param>
        ///// <param name="program">list of programs</param>
        ///// <param name="tactic">list of tactics</param>
        ///// <param name="improvementTactic">list of improvement tactics</param>
        ///// <param name="requestCount">No. of tactic count for Request tab</param>
        ///// <param name="planYear">Plan year</param>
        ///// <returns>Json result, list of task to be rendered in Gantt chart</returns>
        //private JsonResult PrepareVerticalTabResult(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion)
        //{
        //    var verticalTactic = tactic.ToList().Select(pcpt => new
        //    {
        //        PlanTacticId = pcpt.PlanTacticId,
        //        VerticalId = pcpt.VerticalId,
        //        Title = pcpt.Title,
        //        TaskId = string.Format("V{0}_L{1}_C{2}_P{3}_T{4}", pcpt.VerticalId, pcpt.Plan_Campaign_Program.Plan_Campaign.PlanId, pcpt.Plan_Campaign_Program.PlanCampaignId, pcpt.PlanProgramId, pcpt.PlanTacticId),
        //    });

        //    var verticals = (tactic.Select(vertical => vertical.Vertical)).Select(vertical => new
        //    {
        //        VerticalId = vertical.VerticalId,
        //        Title = vertical.Title,
        //        ColorCode = vertical.ColorCode
        //    }).Distinct().OrderBy(vertical => vertical.Title);

        //    List<object> verticalTaskData = GetTaskDetailVertical(campaign.ToList(), program.ToList(), tactic.ToList(), improvementTactic);

        //    return Json(new
        //    {
        //        verticalTactic = verticalTactic.ToList(),
        //        vertical = verticals.ToList(),
        //        taskData = verticalTaskData,
        //        requestCount = requestCount,
        //        planYear = planYear,
        //        improvementTacticForAccordion = improvementTacticForAccordion,
        //        improvementTacticTypeForAccordion = improvementTacticTypeForAccordion
        //    }, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region Prepare Stage Tab data
        ///// <summary>
        ///// Prepare Json result for Stage tab to be rendered in gantt chart
        ///// </summary>
        ///// <param name="campaign">list of campaigns</param>
        ///// <param name="program">list of programs</param>
        ///// <param name="tactic">list of tactics</param>
        ///// <param name="improvementTactic">list of improvement tactics</param>
        ///// <param name="requestCount">No. of tactic count for Request tab</param>
        ///// <param name="planYear">Plan year</param>
        ///// <returns>Json result, list of task to be rendered in Gantt chart</returns>
        //private JsonResult PrepareStageTabResult(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion)
        //{
        //    var queryStages = tactic.Select(t => new
        //    {
        //        t.StageId,
        //        t.Stage.Title,
        //        t.Stage.ColorCode
        //    }).Distinct();

        //    var queryStageTacticType = tactic.Select(t => new
        //    {
        //        t.PlanTacticId,
        //        t.Title,
        //        t.StageId,
        //        TaskId = string.Format("S{0}_L{1}_C{2}_P{3}_T{4}", t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId, t.PlanTacticId)
        //    });

        //    List<object> stageTaskData = GetTaskDetailStage(campaign.ToList(), program.ToList(), tactic.ToList(), improvementTactic);

        //    return Json(new
        //    {
        //        //// For View Control.
        //        stageTactic = queryStageTacticType.ToList(),

        //        //// For View Control.
        //        stage = queryStages.ToList(),

        //        //// For Gantt.
        //        taskData = stageTaskData,
        //        requestCount = requestCount,
        //        planYear = planYear,
        //        improvementTacticForAccordion = improvementTacticForAccordion,
        //        improvementTacticTypeForAccordion = improvementTacticTypeForAccordion
        //    }, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region Prepare Audience Tab data
        ///// <summary>
        ///// Prepare Json result for Audience tab to be rendered in gantt chart
        ///// </summary>
        ///// <param name="campaign">list of campaigns</param>
        ///// <param name="program">list of programs</param>
        ///// <param name="tactic">list of tactics</param>
        ///// <param name="improvementTactic">list of improvement tactics</param>
        ///// <param name="requestCount">No. of tactic count for Request tab</param>
        ///// <param name="planYear">Plan year</param>
        ///// <returns>Json result, list of task to be rendered in Gantt chart</returns>
        //private JsonResult PrepareAudienceTabResult(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion)
        //{
        //    var audienceTactic = tactic.ToList().Select(pcpt => new
        //    {
        //        PlanTacticId = pcpt.PlanTacticId,
        //        AudienceId = pcpt.AudienceId,
        //        Title = pcpt.Title,
        //        TaskId = string.Format("A{0}_L{1}_C{2}_P{3}_T{4}", pcpt.AudienceId, pcpt.Plan_Campaign_Program.Plan_Campaign.PlanId, pcpt.Plan_Campaign_Program.PlanCampaignId, pcpt.PlanProgramId, pcpt.PlanTacticId)
        //    });

        //    var audiences = (tactic.Select(audience => audience.Audience)).Select(audience => new
        //    {
        //        audience.AudienceId,
        //        audience.Title,
        //        ColorCode = audience.ColorCode
        //    }).Distinct().OrderBy(audience => audience.Title);

        //    List<object> audienceTaskData = GetTaskDetailAudience(campaign.ToList(), program.ToList(), tactic.ToList(), improvementTactic);

        //    return Json(new
        //    {
        //        audienceTactic = audienceTactic.ToList(),
        //        audience = audiences.ToList(),
        //        taskData = audienceTaskData,
        //        requestCount = requestCount,
        //        planYear = planYear,
        //        improvementTacticForAccordion = improvementTacticForAccordion,
        //        improvementTacticTypeForAccordion = improvementTacticTypeForAccordion
        //    }, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region Prepare BusinessUnit Tab data
        ///// <summary>
        ///// Prepare Json result for BusinessUnit tab to be rendered in gantt chart
        ///// </summary>
        ///// <param name="campaign">list of campaigns</param>
        ///// <param name="program">list of programs</param>
        ///// <param name="tactic">list of tactics</param>
        ///// <param name="improvementTactic">list of improvement tactics</param>
        ///// <param name="requestCount">No. of tactic count for Request tab</param>
        ///// <param name="planYear">Plan year</param>
        ///// <returns>Json result, list of task to be rendered in Gantt chart</returns>
        //private JsonResult PrepareBusinessUnitTabResult(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion)
        //{
        //    //// Business and Tactic type.
        //    var queryBusinessUnitTacticType = tactic.Select(pt => new
        //    {
        //        pt.PlanTacticId,
        //        pt.Title,
        //        pt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId,// businessUnit.BusinessUnitId,
        //        TaskId = string.Format("B{0}_L{1}_C{2}_P{3}_T{4}", pt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId, pt.Plan_Campaign_Program.Plan_Campaign.PlanId, pt.Plan_Campaign_Program.PlanCampaignId, pt.PlanProgramId, pt.PlanTacticId)
        //    });

        //    var businessUnits = (tactic.Select(bu => bu.BusinessUnit)).Select(bu => new
        //    {
        //        bu.BusinessUnitId,
        //        bu.Title,
        //        ColorCode = bu.ColorCode
        //    }).Distinct().OrderBy(audience => audience.Title);

        //    //// Modified By Maninder Singh Wadhva PL Ticket#47
        //    List<object> businessUnitTaskData = GetTaskDetailBusinessUnit(tactic.ToList(), improvementTactic);

        //    //// Modified By Maninder Singh Wadhva PL Ticket#47
        //    if (queryBusinessUnitTacticType.Count() > 0)
        //    {
        //        return Json(new
        //        {
        //            //// For View Control.
        //            businessUnitTactic = queryBusinessUnitTacticType.ToList(),

        //            //// For View Control.
        //            businessUnit = businessUnits,

        //            //// For Gantt.
        //            taskData = businessUnitTaskData,
        //            requestCount = requestCount,
        //            planYear = planYear,
        //            improvementTacticForAccordion = improvementTacticForAccordion,
        //            improvementTacticTypeForAccordion = improvementTacticTypeForAccordion
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        return Json(new { businessUnitTactic = "", businessUnit = "", taskData = "", planYear = planYear }, JsonRequestBehavior.AllowGet);
        //    }
        //}
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
            var lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
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
        /// Modified By Maninder Singh Wadhva PL Ticket#47
        /// Date: 11/22/2013
        /// Function to get GANTT chart task detail for Business unit.
        /// Modfied By: Maninder Singh Wadhva.
        /// Reason: To show task whose either start or end date or both date are outside current view.
        /// </summary>
        /// <param name="modelBusinessUnit">Model and business unit.</param>
        /// <param name="planTactic">Plan tactic.</param>
        /// <returns>Returns list of task for GANNT CHART.</returns>
        //private List<object> GetTaskDetailBusinessUnit(List<Plan_Campaign_Program_Tactic> planTactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        //{
        //    var campaign = planTactic.Select(c => c.Plan_Campaign_Program.Plan_Campaign).ToList();
        //    var program = planTactic.Select(c => c.Plan_Campaign_Program).ToList();

        //    //// Business Unit.
        //    if (planTactic.Count > 0)
        //    {
        //        var queryBusinessUnitTacticType = planTactic.Select(bu => new
        //        {
        //            id = string.Format("B{0}", bu.BusinessUnitId),
        //            text = bu.BusinessUnit.Title,
        //            start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateStageAndBusinessUnit(campaign, program, planTactic.Where(pt => pt.BusinessUnitId.Equals(bu.BusinessUnitId)).Select(pt => pt).ToList())),
        //            duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, GetMinStartDateStageAndBusinessUnit(campaign, program, planTactic.Where(pt => pt.BusinessUnitId.Equals(bu.BusinessUnitId)).Select(pt => pt).ToList()),
        //                                                      GetMaxEndDateStageAndBusinessUnit(campaign, program, planTactic.Where(pt => pt.BusinessUnitId.Equals(bu.BusinessUnitId)).Select(pt => pt).ToList())),
        //            progress = 0,
        //            //                                   GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.BusinessUnit, bu.PlanTacticId, bu.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, planTactic)),
        //            //                                   Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
        //            //                                            GetMinStartDateForPlan(GanttTabs.BusinessUnit, bu.PlanTacticId, bu.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, planTactic),
        //            //                                            GetMaxEndDateForPlan(GanttTabs.BusinessUnit, bu.PlanTacticId, bu.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, planTactic)), planTactic, improvementTactic),
        //            open = false,
        //            color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, bu.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnit.ColorCode.ToLower())
        //        }).ToList().Distinct().ToList().OrderBy(b => b.text).ToList();

        //        var newQueryBusinessUnitTacticType = queryBusinessUnitTacticType.Select(bu => new
        //        {
        //            id = bu.id,
        //            text = bu.text,
        //            start_date = bu.start_date,
        //            duration = bu.duration,
        //            progress = bu.progress,
        //            open = bu.open,
        //            color = bu.color// + ((bu.progress > 0) ? "stripe" : "")
        //        });

        //        // Start - Added by Sohel Pathan on 19/09/2014 for PL ticket #787
        //        var taskDataPlan = planTactic.Select(t => new
        //        {
        //            id = string.Format("B{0}_L{1}", t.BusinessUnitId, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //            text = t.Plan_Campaign_Program.Plan_Campaign.Plan.Title,
        //            start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.BusinessUnit, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, planTactic)),
        //            duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                      CalendarEndDate,
        //                                                      GetMinStartDateForPlan(GanttTabs.BusinessUnit, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, planTactic),
        //                                                      GetMaxEndDateForPlan(GanttTabs.BusinessUnit, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, planTactic)),
        //            progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.BusinessUnit, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, planTactic)),
        //                                            Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                      CalendarEndDate,
        //                                                      GetMinStartDateForPlan(GanttTabs.BusinessUnit, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, planTactic),
        //                                                      GetMaxEndDateForPlan(GanttTabs.BusinessUnit, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, planTactic)),
        //                                           planTactic, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //            open = false,
        //            parent = string.Format("B{0}", t.BusinessUnitId),
        //            color = GetColorBasedOnImprovementActivity(improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //            planid = t.Plan_Campaign_Program.Plan_Campaign.PlanId
        //        }).Select(t => t).Distinct().OrderBy(t => t.text);

        //        var newTaskDataPlan = taskDataPlan.Select(c => new
        //        {
        //            id = c.id,
        //            text = c.text,
        //            start_date = c.start_date,
        //            duration = c.duration,
        //            progress = c.progress,
        //            open = c.open,
        //            parent = c.parent,
        //            color = c.color + (c.progress > 0 ? "stripe" : ""),
        //            planid = c.planid
        //        });
        //        // End - Added by Sohel Pathan on 19/09/2014 for PL ticket #787

        //        //// Tactic
        //        var taskDataTactic = planTactic.Select(bt => new
        //        {
        //            id = string.Format("B{0}_L{1}_C{2}_P{3}_T{4}", bt.BusinessUnitId, bt.Plan_Campaign_Program.Plan_Campaign.PlanId, bt.Plan_Campaign_Program.PlanCampaignId, bt.Plan_Campaign_Program.PlanProgramId, bt.PlanTacticId),
        //            text = bt.Title,
        //            start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, bt.StartDate),
        //            duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                      CalendarEndDate,
        //                                                      bt.StartDate,
        //                                                      bt.EndDate),
        //            progress = GetTacticProgress(bt, improvementTactic, bt.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //            open = false,
        //            parent = string.Format("B{0}_L{1}_C{2}_P{3}", bt.BusinessUnitId, bt.Plan_Campaign_Program.Plan_Campaign.PlanId, bt.Plan_Campaign_Program.PlanCampaignId, bt.Plan_Campaign_Program.PlanProgramId),
        //            color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, bt.BusinessUnit.ColorCode.ToLower()),
        //            plantacticid = bt.PlanTacticId,
        //            Status = bt.Status      //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //        }).OrderBy(t => t.text);

        //        var newTaskDataTactic = taskDataTactic.Select(t => new
        //        {
        //            id = t.id,
        //            text = t.text,
        //            start_date = t.start_date,
        //            duration = t.duration,
        //            progress = t.progress,
        //            open = t.open,
        //            parent = t.parent,
        //            color = t.color + (t.progress == 1 ? " stripe" : ""),
        //            plantacticid = t.plantacticid,
        //            Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //        });

        //        //// Program
        //        var taskDataProgram = planTactic.Select(bt => new
        //        {
        //            id = string.Format("B{0}_L{1}_C{2}_P{3}", bt.BusinessUnitId, bt.Plan_Campaign_Program.Plan_Campaign.PlanId, bt.Plan_Campaign_Program.PlanCampaignId, bt.Plan_Campaign_Program.PlanProgramId),
        //            text = bt.Plan_Campaign_Program.Title,
        //            start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, bt.Plan_Campaign_Program.StartDate),
        //            duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                      CalendarEndDate,
        //                                                      bt.Plan_Campaign_Program.StartDate,
        //                                                      bt.Plan_Campaign_Program.EndDate),
        //            progress = GetProgramProgress(planTactic, bt.Plan_Campaign_Program, improvementTactic, bt.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //            open = false,
        //            parent = string.Format("B{0}_L{1}_C{2}", bt.BusinessUnitId, bt.Plan_Campaign_Program.Plan_Campaign.PlanId, bt.Plan_Campaign_Program.PlanCampaignId),
        //            color = "",
        //            planprogramid = bt.Plan_Campaign_Program.PlanProgramId,
        //            Status = bt.Plan_Campaign_Program.Status        //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //        }).Select(p => p).Distinct().OrderBy(p => p.text);

        //        var newTaskDataProgram = taskDataProgram.Select(t => new
        //        {
        //            id = t.id,
        //            text = t.text,
        //            start_date = t.start_date,
        //            duration = t.duration,
        //            progress = t.progress,
        //            open = t.open,
        //            parent = t.parent,
        //            color = (t.progress == 1 ? " stripe stripe-no-border " : (t.progress > 0 ? "partialStripe" : "")),
        //            planprogramid = t.planprogramid,
        //            Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //        });

        //        //// Campaign
        //        var taskDataCampaign = planTactic.Select(bt => new
        //        {
        //            id = string.Format("B{0}_L{1}_C{2}", bt.BusinessUnitId, bt.Plan_Campaign_Program.Plan_Campaign.PlanId, bt.Plan_Campaign_Program.PlanCampaignId),
        //            text = bt.Plan_Campaign_Program.Plan_Campaign.Title,
        //            start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, bt.Plan_Campaign_Program.Plan_Campaign.StartDate),
        //            //duration = (bt.Plan_Campaign_Program.Plan_Campaign.EndDate - bt.Plan_Campaign_Program.Plan_Campaign.StartDate).TotalDays,
        //            duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                      CalendarEndDate,
        //                                                      bt.Plan_Campaign_Program.Plan_Campaign.StartDate,
        //                                                      bt.Plan_Campaign_Program.Plan_Campaign.EndDate),
        //            progress = GetCampaignProgress(planTactic, bt.Plan_Campaign_Program.Plan_Campaign, improvementTactic, bt.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //            open = false,
        //            parent = string.Format("B{0}_L{1}", bt.BusinessUnitId, bt.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //            color = GetColorBasedOnImprovementActivity(improvementTactic, bt.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //            plancampaignid = bt.Plan_Campaign_Program.PlanCampaignId,
        //            Status = bt.Plan_Campaign_Program.Plan_Campaign.Status,      //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //        }).Select(c => c).Distinct().ToList().OrderBy(c => c.text);

        //        var newTaskDataCampaign = taskDataCampaign.Select(t => new
        //        {
        //            id = t.id,
        //            text = t.text,
        //            start_date = t.start_date,
        //            duration = t.duration,
        //            progress = t.progress,
        //            open = t.open,
        //            parent = t.parent,
        //            color = t.color + (t.progress == 1 ? " stripe" : (t.progress > 0 ? "stripe" : "")),
        //            plancampaignid = t.plancampaignid,
        //            Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //        });

        //        #region Improvement Activities & Tactics
        //        var improvemntTacticBusinessUnitList = improvementTactic.Select(it => new
        //        {
        //            ImprovementTactic = it,
        //            //// Getting start date for improvement activity task.
        //            minStartDate = improvementTactic.Where(impt => impt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impt => impt.EffectiveDate).Min(),
        //        }).Select(it => it).ToList().Distinct().ToList();

        //        //// Improvement Activities
        //        var taskDataImprovementActivity = improvemntTacticBusinessUnitList.Select(ita => new
        //        {
        //            id = string.Format("B{0}_L{1}_M{2}", ita.ImprovementTactic.BusinessUnitId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId),
        //            text = ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Title,
        //            start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, ita.minStartDate),
        //            duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                      CalendarEndDate,
        //                                                      ita.minStartDate,
        //                                                      CalendarEndDate) - 1,
        //            progress = 0,
        //            open = true,
        //            color = GetColorBasedOnImprovementActivity(improvementTactic, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
        //            ImprovementActivityId = ita.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
        //            isImprovement = true,
        //            IsHideDragHandleLeft = ita.minStartDate < CalendarStartDate,
        //            IsHideDragHandleRight = true,
        //            parent = string.Format("B{0}_L{1}", ita.ImprovementTactic.BusinessUnitId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
        //        }).Select(i => i).Distinct().ToList();

        //        //// Improvent Tactics
        //        string tacticStatusSubmitted = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
        //        string tacticStatusDeclined = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;

        //        var taskDataImprovementTactic = improvemntTacticBusinessUnitList.Select(it => new
        //        {
        //            id = string.Format("B{0}_L{1}_M{2}_I{3}_Y{4}", it.ImprovementTactic.BusinessUnitId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId, it.ImprovementTactic.ImprovementPlanTacticId, it.ImprovementTactic.ImprovementTacticTypeId),
        //            text = it.ImprovementTactic.Title,
        //            start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, it.ImprovementTactic.EffectiveDate),
        //            duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                      CalendarEndDate,
        //                                                      it.ImprovementTactic.EffectiveDate,
        //                                                      CalendarEndDate) - 1,
        //            progress = 0,
        //            open = true,
        //            parent = string.Format("B{0}_L{1}_M{2}", it.ImprovementTactic.BusinessUnitId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId),
        //            color = string.Concat(Common.GANTT_BAR_CSS_CLASS_PREFIX_IMPROVEMENT, it.ImprovementTactic.ImprovementTacticType.ColorCode.ToLower()),
        //            isSubmitted = it.ImprovementTactic.Status.Equals(tacticStatusSubmitted),
        //            isDeclined = it.ImprovementTactic.Status.Equals(tacticStatusDeclined),
        //            inqs = 0,
        //            mqls = 0,
        //            cost = it.ImprovementTactic.Cost,
        //            cws = 0,
        //            it.ImprovementTactic.ImprovementPlanTacticId,
        //            isImprovement = true,
        //            IsHideDragHandleLeft = it.ImprovementTactic.EffectiveDate < CalendarStartDate,
        //            IsHideDragHandleRight = true,
        //            Status = it.ImprovementTactic.Status
        //        }).OrderBy(t => t.text);
        //        #endregion

        //        return newQueryBusinessUnitTacticType.Concat<object>(newTaskDataPlan).Concat<object>(taskDataImprovementActivity).Concat<object>(taskDataImprovementTactic).Concat<object>(newTaskDataCampaign).Concat<object>(newTaskDataProgram).Concat<object>(newTaskDataTactic).ToList<object>();
        //    }
        //    else
        //        return null;

        //}

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/18/2013
        /// Function to get GANTT chart task detail for audience.
        /// Modfied By: Maninder Singh Wadhva.
        /// Reason: To show task whose either start or end date or both date are outside current view.
        /// </summary>
        /// <param name="campaign">Campaign of plan version.</param>
        /// <param name="program">Program of plan version.</param>
        /// <param name="tactic">Tactic of plan version.</param>
        /// <returns>Returns list of task for GANNT CHART.</returns>
        //public List<object> GetTaskDetailAudience(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        //{
        //    var taskDataAudience = tactic.Select(t => new
        //    {
        //        id = string.Format("A{0}", t.AudienceId),
        //        text = t.Audience.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDate(GanttTabs.Audience, t.AudienceId, campaign, program, tactic)),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                         CalendarEndDate,
        //                                         GetMinStartDate(GanttTabs.Audience, t.AudienceId, campaign, program, tactic),
        //                                                  GetMaxEndDate(GanttTabs.Audience, t.AudienceId, campaign, program, tactic)),
        //        progress = 0,
        //        //GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDate(GanttTabs.Audience, t.AudienceId, campaign, program, tactic)),
        //        //                               Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //        //                                         CalendarEndDate,
        //        //                                         GetMinStartDate(GanttTabs.Audience, t.AudienceId, campaign, program, tactic),
        //        //                                         GetMaxEndDate(GanttTabs.Audience, t.AudienceId, campaign, program, tactic)), tactic, improvementTactic,
        //        //                                         t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Audience.ColorCode.ToLower())
        //    }).Select(a => a).Distinct().OrderBy(t => t.text);

        //    var newTaskDataAudience = taskDataAudience.Select(t => new
        //    {
        //        id = t.id,
        //        text = t.text,
        //        start_date = t.start_date,
        //        duration = t.duration,
        //        progress = t.progress,
        //        open = t.open,
        //        color = t.color,// + ((t.progress > 0) ? "stripe" : "")
        //    });

        //    // Start - Added by Sohel Pathan on 18/09/2014 for PL ticket #787
        //    var taskDataPlan = tactic.Select(t => new
        //    {
        //        id = string.Format("A{0}_L{1}", t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        text = t.Plan_Campaign_Program.Plan_Campaign.Plan.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Audience, t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  GetMinStartDateForPlan(GanttTabs.Audience, t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic),
        //                                                  GetMaxEndDateForPlan(GanttTabs.Audience, t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
        //        progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Audience, t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
        //                                        Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  GetMinStartDateForPlan(GanttTabs.Audience, t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic),
        //                                                  GetMaxEndDateForPlan(GanttTabs.Audience, t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
        //                                       tactic, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        parent = string.Format("A{0}", t.AudienceId),
        //        color = GetColorBasedOnImprovementActivity(improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        planid = t.Plan_Campaign_Program.Plan_Campaign.PlanId
        //    }).Select(t => t).Distinct().OrderBy(t => t.text);

        //    var newTaskDataPlan = taskDataPlan.Select(c => new
        //    {
        //        id = c.id,
        //        text = c.text,
        //        start_date = c.start_date,
        //        duration = c.duration,
        //        progress = c.progress,
        //        open = c.open,
        //        parent = c.parent,
        //        color = c.color + (c.progress > 0 ? "stripe" : ""),
        //        planid = c.planid
        //    });
        //    // End - Added by Sohel Pathan on 18/09/2014 for PL ticket #787

        //    var taskDataCampaign = tactic.Select(t => new
        //    {
        //        id = string.Format("A{0}_L{1}_C{2}", t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId),  // Modified by Sohel Pathan on 18/09/2014 for PL ticket #787
        //        text = t.Plan_Campaign_Program.Plan_Campaign.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.Plan_Campaign.StartDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  t.Plan_Campaign_Program.Plan_Campaign.StartDate,
        //                                                  t.Plan_Campaign_Program.Plan_Campaign.EndDate),
        //        progress = GetCampaignProgress(tactic, t.Plan_Campaign_Program.Plan_Campaign, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        parent = string.Format("A{0}_L{1}", t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId),   // Modified by Sohel Pathan on 18/09/2014 for PL ticket #787
        //        color = GetColorBasedOnImprovementActivity(improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        plancampaignid = t.Plan_Campaign_Program.PlanCampaignId,
        //        Status = t.Plan_Campaign_Program.Plan_Campaign.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    }).Select(t => t).Distinct().OrderBy(t => t.text);

        //    var newTaskDataCampaign = taskDataCampaign.Select(t => new
        //    {
        //        id = t.id,
        //        text = t.text,
        //        start_date = t.start_date,
        //        duration = t.duration,
        //        progress = t.progress,
        //        open = t.open,
        //        parent = t.parent,
        //        color = t.color + (t.progress == 1 ? " stripe" : (t.progress > 0 ? "stripe" : "")),
        //        plancampaignid = t.plancampaignid,
        //        Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    });

        //    var taskDataProgram = tactic.Select(t => new
        //    {
        //        id = string.Format("A{0}_L{1}_C{2}_P{3}", t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),    // Modified by Sohel Pathan on 18/09/2014 for PL ticket #787
        //        text = t.Plan_Campaign_Program.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.StartDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  t.Plan_Campaign_Program.StartDate,
        //                                                  t.Plan_Campaign_Program.EndDate),
        //        progress = GetProgramProgress(tactic, t.Plan_Campaign_Program, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        parent = string.Format("A{0}_L{1}_C{2}", t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId),  // Modified by Sohel Pathan on 18/09/2014 for PL ticket #787
        //        color = "",
        //        planprogramid = t.PlanProgramId,
        //        Status = t.Plan_Campaign_Program.Status     //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    }).Select(t => t).Distinct().OrderBy(t => t.text);


        //    var newTaskDataProgram = taskDataProgram.Select(t => new
        //    {
        //        id = t.id,
        //        text = t.text,
        //        start_date = t.start_date,
        //        duration = t.duration,
        //        progress = t.progress,
        //        open = t.open,
        //        parent = t.parent,
        //        color = (t.progress == 1 ? " stripe stripe-no-border " : (t.progress > 0 ? "partialStripe" : "")),
        //        planprogramid = t.planprogramid,
        //        Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    });

        //    var taskDataTactic = tactic.Select(t => new
        //    {
        //        id = string.Format("A{0}_L{1}_C{2}_P{3}_T{4}", t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId), // Modified by Sohel Pathan on 18/09/2014 for PL ticket #787
        //        text = t.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  t.StartDate,
        //                                                  t.EndDate),
        //        progress = GetTacticProgress(t, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        parent = string.Format("A{0}_L{1}_C{2}_P{3}", t.AudienceId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),    // Modified by Sohel Pathan on 18/09/2014 for PL ticket #787
        //        color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Audience.ColorCode.ToLower()),
        //        plantacticid = t.PlanTacticId,
        //        Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    }).OrderBy(t => t.text);

        //    var newTaskDataTactic = taskDataTactic.Select(t => new
        //    {
        //        id = t.id,
        //        text = t.text,
        //        start_date = t.start_date,
        //        duration = t.duration,
        //        progress = t.progress,
        //        open = t.open,
        //        parent = t.parent,
        //        color = t.color + (t.progress == 1 ? " stripe" : ""),
        //        plantacticid = t.plantacticid,
        //        Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    });

        //    // Start - Added by Sohel Pathan on 18/09/2014 for PL ticket #787
        //    #region Improvement Activities & Tactics
        //    var improvemntTacticAudienceList = (from it in improvementTactic
        //                                        join t in tactic on it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals t.Plan_Campaign_Program.Plan_Campaign.PlanId
        //                                        join a in db.Audiences on t.AudienceId equals a.AudienceId
        //                                        where it.IsDeleted == false && t.IsDeleted == false && a.IsDeleted == false
        //                                        select new
        //                                        {
        //                                            ImprovementTactic = it,
        //                                            AudienceId = a.AudienceId,
        //                                            //// Getting start date for improvement activity task.
        //                                            minStartDate = improvementTactic.Where(impt => impt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impt => impt.EffectiveDate).Min(),
        //                                        }).Select(it => it).ToList().Distinct().ToList();

        //    //// Improvement Activities
        //    var taskDataImprovementActivity = improvemntTacticAudienceList.Select(ita => new
        //    {
        //        id = string.Format("A{0}_L{1}_M{2}", ita.AudienceId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId),
        //        text = ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, ita.minStartDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  ita.minStartDate,
        //                                                  CalendarEndDate) - 1,
        //        progress = 0,
        //        open = true,
        //        color = GetColorBasedOnImprovementActivity(improvementTactic, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
        //        ImprovementActivityId = ita.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
        //        isImprovement = true,
        //        IsHideDragHandleLeft = ita.minStartDate < CalendarStartDate,
        //        IsHideDragHandleRight = true,
        //        parent = string.Format("A{0}_L{1}", ita.AudienceId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
        //    }).Select(i => i).Distinct().ToList();

        //    //// Improvent Tactics
        //    string tacticStatusSubmitted = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
        //    string tacticStatusDeclined = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;

        //    var taskDataImprovementTactic = improvemntTacticAudienceList.Select(it => new
        //    {
        //        id = string.Format("A{0}_L{1}_M{2}_I{3}_Y{4}", it.AudienceId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId, it.ImprovementTactic.ImprovementPlanTacticId, it.ImprovementTactic.ImprovementTacticTypeId),
        //        text = it.ImprovementTactic.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, it.ImprovementTactic.EffectiveDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  it.ImprovementTactic.EffectiveDate,
        //                                                  CalendarEndDate) - 1,
        //        progress = 0,
        //        open = true,
        //        parent = string.Format("A{0}_L{1}_M{2}", it.AudienceId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId),
        //        color = string.Concat(Common.GANTT_BAR_CSS_CLASS_PREFIX_IMPROVEMENT, it.ImprovementTactic.ImprovementTacticType.ColorCode.ToLower()),
        //        isSubmitted = it.ImprovementTactic.Status.Equals(tacticStatusSubmitted),
        //        isDeclined = it.ImprovementTactic.Status.Equals(tacticStatusDeclined),
        //        inqs = 0,
        //        mqls = 0,
        //        cost = it.ImprovementTactic.Cost,
        //        cws = 0,
        //        it.ImprovementTactic.ImprovementPlanTacticId,
        //        isImprovement = true,
        //        IsHideDragHandleLeft = it.ImprovementTactic.EffectiveDate < CalendarStartDate,
        //        IsHideDragHandleRight = true,
        //        Status = it.ImprovementTactic.Status
        //    }).OrderBy(t => t.text);
        //    #endregion
        //    // End - Added by Sohel Pathan on 18/09/2014 for PL ticket #787

        //    return newTaskDataAudience.Concat<object>(newTaskDataPlan).Concat<object>(taskDataImprovementActivity).Concat<object>(taskDataImprovementTactic).Concat<object>(newTaskDataCampaign).Concat<object>(newTaskDataTactic).Concat<object>(newTaskDataProgram).ToList<object>(); // Modified by Sohel Pathan on 18/09/2014 for PL ticket #787
        //}

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/22/2013
        /// Function to get GANTT chart task detail for Stage.
        /// Modfied By: Maninder Singh Wadhva.
        /// Reason: To show task whose either start or end date or both date are outside current view.
        /// Modified by Maninder Singh Wadhva on 06/13/2014 for PL #526 Home & Plan Pages: Stages are different but all Tactics are displayed under SUS
        /// </summary>
        /// <param name="campaign">Campaign of plan version.</param>
        /// <param name="program">Program of plan version.</param>
        /// <param name="tactic">Tactic of plan version.</param>
        /// <returns>Returns list of task for GANNT CHART.</returns>
        //public List<object> GetTaskDetailStage(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        //{
        //    //// Stage
        //    var taskStages = tactic.Select(t => new
        //    {
        //        id = string.Format("S{0}", t.StageId),
        //        text = t.Stage.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(t.StageId)).ToList<Plan_Campaign_Program_Tactic>())),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                          CalendarEndDate,
        //                                          GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(t.StageId)).ToList<Plan_Campaign_Program_Tactic>()),
        //                                          GetMaxEndDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(t.StageId)).ToList<Plan_Campaign_Program_Tactic>())),
        //        progress = 0,
        //        //GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(t.StageId)).ToList<Plan_Campaign_Program_Tactic>())),
        //        //                            Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //        //                                          CalendarEndDate,
        //        //                                          GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(t.StageId)).ToList<Plan_Campaign_Program_Tactic>()),
        //        //                                          GetMaxEndDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(t.StageId)).ToList<Plan_Campaign_Program_Tactic>())),
        //        //                                          tactic, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Stage.ColorCode.ToLower())
        //    }).Distinct();

        //    var newTaskStages = taskStages.Select(s => new
        //    {
        //        id = s.id,
        //        text = s.text,
        //        start_date = s.start_date,
        //        duration = s.duration,
        //        progress = s.progress,
        //        open = s.open,
        //        color = s.color, // + ((s.progress > 0) ? "stripe" : "")
        //    });

        //    // Start - Added by Sohel Pathan on 16/09/2014 for PL ticket #786
        //    //// Plan
        //    var taskDataPlan = tactic.Select(t => new
        //    {
        //        id = string.Format("S{0}_L{1}", t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        text = t.Plan_Campaign_Program.Plan_Campaign.Plan.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Stage, t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  GetMinStartDateForPlan(GanttTabs.Stage, t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic),
        //                                                  GetMaxEndDateForPlan(GanttTabs.Stage, t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
        //        progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Stage, t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
        //                                        Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  GetMinStartDateForPlan(GanttTabs.Stage, t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic),
        //                                                  GetMaxEndDateForPlan(GanttTabs.Stage, t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
        //                                          tactic, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        parent = string.Format("S{0}", t.StageId),
        //        color = GetColorBasedOnImprovementActivity(improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        planid = t.Plan_Campaign_Program.Plan_Campaign.PlanId
        //    }).Select(c => c).Distinct().OrderBy(c => c.text);

        //    var newTaskDataPlan = taskDataPlan.Select(c => new
        //    {
        //        id = c.id,
        //        text = c.text,
        //        start_date = c.start_date,
        //        duration = c.duration,
        //        progress = c.progress,
        //        open = c.open,
        //        parent = c.parent,
        //        color = c.color + (c.progress > 0 ? "stripe" : ""),
        //        planid = c.planid
        //    });
        //    // End - Added by Sohel Pathan on 16/09/2014 for PL ticket #786

        //    //// Tactic
        //    var taskDataTactic = tactic.Select(t => new
        //    {
        //        id = string.Format("S{0}_L{1}_C{2}_P{3}_T{4}", t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId),    // Modified by Sohel Pathan on 16/09/2014 for PL ticket #786
        //        text = t.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  t.StartDate,
        //                                                  t.EndDate),
        //        progress = GetTacticProgress(t, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        parent = string.Format("S{0}_L{1}_C{2}_P{3}", t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),   // Modified by Sohel Pathan on 16/09/2014 for PL ticket #786
        //        color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Stage.ColorCode.ToLower()),
        //        plantacticid = t.PlanTacticId,
        //        Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    }).OrderBy(t => t.text);

        //    var newTaskDataTactic = taskDataTactic.Select(t => new
        //    {
        //        id = t.id,
        //        text = t.text,
        //        start_date = t.start_date,
        //        duration = t.duration,
        //        progress = t.progress,
        //        open = t.open,
        //        parent = t.parent,
        //        color = t.color + (t.progress == 1 ? " stripe" : ""),
        //        plantacticid = t.plantacticid,
        //        Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    });

        //    //// Program
        //    var taskDataProgram = tactic.Select(t => new
        //    {
        //        id = string.Format("S{0}_L{1}_C{2}_P{3}", t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),   // Modified by Sohel Pathan on 16/09/2014 for PL ticket #786
        //        text = t.Plan_Campaign_Program.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.StartDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  t.Plan_Campaign_Program.StartDate,
        //                                                  t.Plan_Campaign_Program.EndDate),
        //        progress = GetProgramProgress(tactic, t.Plan_Campaign_Program, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        parent = string.Format("S{0}_L{1}_C{2}", t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId), // Modified by Sohel Pathan on 16/09/2014 for PL ticket #786
        //        color = "",
        //        planprogramid = t.PlanProgramId,
        //        Status = t.Plan_Campaign_Program.Status     //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    }).Select(p => p).Distinct().OrderBy(p => p.text);

        //    var newTaskDataProgram = taskDataProgram.Select(p => new
        //    {
        //        id = p.id,
        //        text = p.text,
        //        start_date = p.start_date,
        //        duration = p.duration,
        //        progress = p.progress,
        //        open = p.open,
        //        parent = p.parent,
        //        color = (p.progress == 1 ? " stripe stripe-no-border " : (p.progress > 0 ? "partialStripe" : "")),
        //        planprogramid = p.planprogramid,
        //        Status = p.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    });

        //    //// Campaign
        //    var taskDataCampaign = tactic.Select(t => new
        //    {
        //        id = string.Format("S{0}_L{1}_C{2}", t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId), // Modified by Sohel Pathan on 16/09/2014 for PL ticket #786
        //        text = t.Plan_Campaign_Program.Plan_Campaign.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.Plan_Campaign.StartDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  t.Plan_Campaign_Program.Plan_Campaign.StartDate,
        //                                                  t.Plan_Campaign_Program.Plan_Campaign.EndDate),
        //        progress = GetCampaignProgress(tactic, t.Plan_Campaign_Program.Plan_Campaign, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        parent = string.Format("S{0}_L{1}", t.StageId, t.Plan_Campaign_Program.Plan_Campaign.PlanId),  // Modified by Sohel Pathan on 16/09/2014 for PL ticket #786
        //        color = GetColorBasedOnImprovementActivity(improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        plancampaignid = t.Plan_Campaign_Program.PlanCampaignId,
        //        Status = t.Plan_Campaign_Program.Plan_Campaign.Status,       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    }).Select(c => c).Distinct().ToList().OrderBy(c => c.text);

        //    var newTaskDataCampaign = taskDataCampaign.Select(c => new
        //    {
        //        id = c.id,
        //        text = c.text,
        //        start_date = c.start_date,
        //        duration = c.duration,
        //        progress = c.progress,
        //        open = c.open,
        //        parent = c.parent,
        //        color = c.color + (c.progress == 1 ? " stripe" : (c.progress > 0 ? "stripe" : "")),
        //        plancampaignid = c.plancampaignid,
        //        Status = c.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    });

        //    #region Improvement Activities & Tactics
        //    var improvemntTacticStageList = (from it in improvementTactic
        //                                     join t in tactic on it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals t.Plan_Campaign_Program.Plan_Campaign.PlanId
        //                                     join s in db.Stages on t.StageId equals s.StageId
        //                                     where it.IsDeleted == false && t.IsDeleted == false && s.IsDeleted == false
        //                                     select new
        //                                     {
        //                                         ImprovementTactic = it,
        //                                         StageId = s.StageId,
        //                                         //// Getting start date for improvement activity task.
        //                                         minStartDate = improvementTactic.Where(impt => impt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impt => impt.EffectiveDate).Min(),
        //                                     }).Select(a => a).ToList().Distinct().ToList();

        //    //// Improvement Activities
        //    var taskDataImprovementActivity = improvemntTacticStageList.Select(ita => new
        //    {
        //        id = string.Format("S{0}_L{1}_M{2}", ita.StageId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId),
        //        text = ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, ita.minStartDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  ita.minStartDate,
        //                                                  CalendarEndDate) - 1,
        //        progress = 0,
        //        open = true,
        //        color = GetColorBasedOnImprovementActivity(improvementTactic, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
        //        ImprovementActivityId = ita.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
        //        isImprovement = true,
        //        IsHideDragHandleLeft = ita.minStartDate < CalendarStartDate,
        //        IsHideDragHandleRight = true,
        //        parent = string.Format("S{0}_L{1}", ita.StageId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
        //    }).Select(i => i).Distinct().ToList();

        //    //// Improvent Tactics
        //    string tacticStatusSubmitted = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
        //    string tacticStatusDeclined = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;

        //    var taskDataImprovementTactic = improvemntTacticStageList.Select(it => new
        //    {
        //        id = string.Format("S{0}_L{1}_M{2}_I{3}_Y{4}", it.StageId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId, it.ImprovementTactic.ImprovementPlanTacticId, it.ImprovementTactic.ImprovementTacticTypeId),
        //        text = it.ImprovementTactic.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, it.ImprovementTactic.EffectiveDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  it.ImprovementTactic.EffectiveDate,
        //                                                  CalendarEndDate) - 1,
        //        progress = 0,
        //        open = true,
        //        parent = string.Format("S{0}_L{1}_M{2}", it.StageId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId),
        //        color = string.Concat(Common.GANTT_BAR_CSS_CLASS_PREFIX_IMPROVEMENT, it.ImprovementTactic.ImprovementTacticType.ColorCode.ToLower()),
        //        isSubmitted = it.ImprovementTactic.Status.Equals(tacticStatusSubmitted),
        //        isDeclined = it.ImprovementTactic.Status.Equals(tacticStatusDeclined),
        //        inqs = 0,
        //        mqls = 0,
        //        cost = it.ImprovementTactic.Cost,
        //        cws = 0,
        //        it.ImprovementTactic.ImprovementPlanTacticId,
        //        isImprovement = true,
        //        IsHideDragHandleLeft = it.ImprovementTactic.EffectiveDate < CalendarStartDate,
        //        IsHideDragHandleRight = true,
        //        Status = it.ImprovementTactic.Status
        //    }).OrderBy(t => t.text);
        //    #endregion

        //    return newTaskStages.Concat<object>(newTaskDataPlan).Concat<object>(taskDataImprovementActivity).Concat<object>(taskDataImprovementTactic).Concat<object>(newTaskDataCampaign).Concat<object>(newTaskDataProgram).Concat<object>(newTaskDataTactic).ToList<object>(); // Modified by Sohel Pathan on 16/09/2014 for PL ticket #786
        //}

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/18/2013
        /// Function to get GANTT chart task detail for Vertical.
        /// Modfied By: Maninder Singh Wadhva.
        /// Reason: To show task whose either start or end date or both date are outside current view.
        /// </summary>
        /// <param name="campaign">Campaign of plan version.</param>
        /// <param name="program">Program of plan version.</param>
        /// <param name="tactic">Tactic of plan version.</param>
        /// <returns>Returns list of task for GANNT CHART.</returns>
        //public List<object> GetTaskDetailVertical(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        //{
        //    var taskDataVertical = tactic.Select(t => new
        //    {
        //        id = string.Format("V{0}", t.VerticalId),
        //        text = t.Vertical.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDate(GanttTabs.Vertical, t.VerticalId, campaign, program, tactic)),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                          CalendarEndDate,
        //                                          GetMinStartDate(GanttTabs.Vertical, t.VerticalId, campaign, program, tactic),
        //                                          GetMaxEndDate(GanttTabs.Vertical, t.VerticalId, campaign, program, tactic)),

        //        progress = 0,
        //        //GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDate(GanttTabs.Vertical, t.VerticalId, campaign, program, tactic)),
        //        //                                Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //        //                                          CalendarEndDate,
        //        //                                          GetMinStartDate(GanttTabs.Vertical, t.VerticalId, campaign, program, tactic),
        //        //                                          GetMaxEndDate(GanttTabs.Vertical, t.VerticalId, campaign, program, tactic)),
        //        //                               tactic, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,

        //        color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Vertical.ColorCode.ToLower())
        //    }).Select(v => v).Distinct().OrderBy(t => t.text);

        //    var newTaskDataVertical = taskDataVertical.Select(v => new
        //    {
        //        id = v.id,
        //        text = v.text,
        //        start_date = v.start_date,
        //        duration = v.duration,
        //        progress = v.progress,
        //        open = v.open,
        //        color = v.color + ((v.progress > 0) ? "stripe" : "")
        //    });

        //    // Start - Added by Sohel Pathan on 15/09/2014 for PL ticket #786
        //    var taskDataPlan = tactic.Select(t => new
        //    {
        //        id = string.Format("V{0}_L{1}", t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        text = t.Plan_Campaign_Program.Plan_Campaign.Plan.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Vertical, t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  GetMinStartDateForPlan(GanttTabs.Vertical, t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic),
        //                                                  GetMaxEndDateForPlan(GanttTabs.Vertical, t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
        //        progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Vertical, t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
        //                                        Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  GetMinStartDateForPlan(GanttTabs.Vertical, t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic),
        //                                                  GetMaxEndDateForPlan(GanttTabs.Vertical, t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
        //                               tactic, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        parent = string.Format("V{0}", t.VerticalId),
        //        color = GetColorBasedOnImprovementActivity(improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        planid = t.Plan_Campaign_Program.Plan_Campaign.PlanId
        //    }).Select(t => t).Distinct().OrderBy(t => t.text);

        //    var newTaskDataPlan = taskDataPlan.Select(c => new
        //    {
        //        id = c.id,
        //        text = c.text,
        //        start_date = c.start_date,
        //        duration = c.duration,
        //        progress = c.progress,
        //        open = c.open,
        //        parent = c.parent,
        //        color = c.color + (c.progress > 0 ? "stripe" : ""),
        //        planid = c.planid
        //    });
        //    // End - Added by Sohel Pathan on 15/09/2014 for PL ticket #786

        //    var taskDataCampaign = tactic.Select(t => new
        //    {
        //        id = string.Format("V{0}_L{1}_C{2}", t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId),  // Modified by Sohel Pathan on 15/09/2014 for PL ticket #786
        //        text = t.Plan_Campaign_Program.Plan_Campaign.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.Plan_Campaign.StartDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  t.Plan_Campaign_Program.Plan_Campaign.StartDate,
        //                                                  t.Plan_Campaign_Program.Plan_Campaign.EndDate),
        //        progress = GetCampaignProgress(tactic, t.Plan_Campaign_Program.Plan_Campaign, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        parent = string.Format("V{0}_L{1}", t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId),   // Modified by Sohel Pathan on 15/09/2014 for PL ticket #786
        //        color = GetColorBasedOnImprovementActivity(improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        plancampaignid = t.Plan_Campaign_Program.PlanCampaignId,
        //        Status = t.Plan_Campaign_Program.Plan_Campaign.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    }).Select(t => t).Distinct().OrderBy(t => t.text);

        //    var newTaskDataCampaign = taskDataCampaign.Select(c => new
        //    {
        //        id = c.id,
        //        text = c.text,
        //        start_date = c.start_date,
        //        duration = c.duration,
        //        progress = c.progress,
        //        open = c.open,
        //        parent = c.parent,
        //        color = c.color + (c.progress == 1 ? " stripe" : (c.progress > 0 ? "stripe" : "")),
        //        plancampaignid = c.plancampaignid,
        //        Status = c.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    });

        //    var taskDataProgram = tactic.Select(t => new
        //    {
        //        id = string.Format("V{0}_L{1}_C{2}_P{3}", t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId), // Modified by Sohel Pathan on 15/09/2014 for PL ticket #786
        //        text = t.Plan_Campaign_Program.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.StartDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                   CalendarEndDate,
        //                                                   t.Plan_Campaign_Program.StartDate,
        //                                                   t.Plan_Campaign_Program.EndDate),
        //        progress = GetProgramProgress(tactic, t.Plan_Campaign_Program, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        parent = string.Format("V{0}_L{1}_C{2}", t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId),   // Modified by Sohel Pathan on 15/09/2014 for PL ticket #786
        //        color = "",
        //        planprogramid = t.PlanProgramId,
        //        Status = t.Plan_Campaign_Program.Status      //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    }).Select(t => t).Distinct().OrderBy(t => t.text);

        //    var newTaskDataProgram = taskDataProgram.Select(p => new
        //    {
        //        id = p.id,
        //        text = p.text,
        //        start_date = p.start_date,
        //        duration = p.duration,
        //        progress = p.progress,
        //        open = p.open,
        //        parent = p.parent,
        //        color = (p.progress == 1 ? " stripe stripe-no-border " : (p.progress > 0 ? "partialStripe" : "")),
        //        planprogramid = p.planprogramid,
        //        Status = p.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    });

        //    var taskDataTactic = tactic.Select(t => new
        //    {
        //        id = string.Format("V{0}_L{1}_C{2}_P{3}_T{4}", t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId), // Modified by Sohel Pathan on 15/09/2014 for PL ticket #786
        //        text = t.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  t.StartDate,
        //                                                  t.EndDate),
        //        progress = GetTacticProgress(t, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
        //        open = false,
        //        parent = string.Format("V{0}_L{1}_C{2}_P{3}", t.VerticalId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),    // Modified by Sohel Pathan on 15/09/2014 for PL ticket #786
        //        color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Vertical.ColorCode.ToLower()),
        //        plantacticid = t.PlanTacticId,
        //        Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    }).OrderBy(t => t.text);

        //    var newTaskDataTactic = taskDataTactic.Select(t => new
        //    {
        //        id = t.id,
        //        text = t.text,
        //        start_date = t.start_date,
        //        duration = t.duration,
        //        progress = t.progress,
        //        open = t.open,
        //        parent = t.parent,
        //        color = t.color + (t.progress == 1 ? " stripe" : ""),
        //        plantacticid = t.plantacticid,
        //        Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
        //    });

        //    #region Improvement Activities & Tactics
        //    var improvemntTacticVerticalList = (from it in improvementTactic
        //                                        join t in tactic on it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals t.Plan_Campaign_Program.Plan_Campaign.PlanId
        //                                        join v in db.Verticals on t.VerticalId equals v.VerticalId
        //                                        where it.IsDeleted == false && t.IsDeleted == false && v.IsDeleted == false
        //                                        select new
        //                                        {
        //                                            ImprovementTactic = it,
        //                                            VerticalId = v.VerticalId,
        //                                            //// Getting start date for improvement activity task.
        //                                            minStartDate = improvementTactic.Where(impt => impt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impt => impt.EffectiveDate).Min(),
        //                                        }).Select(it => it).ToList().Distinct().ToList();

        //    //// Improvement Activities
        //    var taskDataImprovementActivity = improvemntTacticVerticalList.Select(ita => new
        //    {
        //        id = string.Format("V{0}_L{1}_M{2}", ita.VerticalId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId),
        //        text = ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, ita.minStartDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  ita.minStartDate,
        //                                                  CalendarEndDate) - 1,
        //        progress = 0,
        //        open = true,
        //        color = GetColorBasedOnImprovementActivity(improvementTactic, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
        //        ImprovementActivityId = ita.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
        //        isImprovement = true,
        //        IsHideDragHandleLeft = ita.minStartDate < CalendarStartDate,
        //        IsHideDragHandleRight = true,
        //        parent = string.Format("V{0}_L{1}", ita.VerticalId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
        //    }).Select(i => i).Distinct().ToList();

        //    //// Improvent Tactics
        //    string tacticStatusSubmitted = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
        //    string tacticStatusDeclined = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;

        //    var taskDataImprovementTactic = improvemntTacticVerticalList.Select(it => new
        //    {
        //        id = string.Format("V{0}_L{1}_M{2}_I{3}_Y{4}", it.VerticalId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId, it.ImprovementTactic.ImprovementPlanTacticId, it.ImprovementTactic.ImprovementTacticTypeId),
        //        text = it.ImprovementTactic.Title,
        //        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, it.ImprovementTactic.EffectiveDate),
        //        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
        //                                                  CalendarEndDate,
        //                                                  it.ImprovementTactic.EffectiveDate,
        //                                                  CalendarEndDate) - 1,
        //        progress = 0,
        //        open = true,
        //        parent = string.Format("V{0}_L{1}_M{2}", it.VerticalId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId),
        //        color = string.Concat(Common.GANTT_BAR_CSS_CLASS_PREFIX_IMPROVEMENT, it.ImprovementTactic.ImprovementTacticType.ColorCode.ToLower()),
        //        isSubmitted = it.ImprovementTactic.Status.Equals(tacticStatusSubmitted),
        //        isDeclined = it.ImprovementTactic.Status.Equals(tacticStatusDeclined),
        //        inqs = 0,
        //        mqls = 0,
        //        cost = it.ImprovementTactic.Cost,
        //        cws = 0,
        //        it.ImprovementTactic.ImprovementPlanTacticId,
        //        isImprovement = true,
        //        IsHideDragHandleLeft = it.ImprovementTactic.EffectiveDate < CalendarStartDate,
        //        IsHideDragHandleRight = true,
        //        Status = it.ImprovementTactic.Status
        //    }).OrderBy(t => t.text);
        //    #endregion

        //    return newTaskDataVertical.Concat<object>(newTaskDataPlan).Concat<object>(taskDataImprovementActivity).Concat<object>(taskDataImprovementTactic).Concat<object>(newTaskDataCampaign).Concat<object>(newTaskDataTactic).Concat<object>(newTaskDataProgram).ToList<object>();   // Modified by Sohel Pathan on 15/09/2014 for PL ticket #786
        //}

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
        public List<object> GetTaskDetailTactic(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        {
            string tacticStatusSubmitted = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            string tacticStatusDeclined = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;
            // Added BY Bhavesh
            // Calculate MQL at runtime #376
            List<TacticStageValue> tacticStageRelationList = Common.GetTacticStageRelation(tactic, false);
            var stageList = db.Stages.Where(s => s.ClientId == Sessions.User.ClientId).ToList();
            string inqstage = Enums.Stage.INQ.ToString();
            string mqlstage = Enums.Stage.MQL.ToString();
            int inqLevel = Convert.ToInt32(stageList.Single(s => s.Code == inqstage).Level);
            int mqlLevel = Convert.ToInt32(stageList.Single(s => s.Code == mqlstage).Level);
            var taskDataTactic = tactic.Select(t => new
            {
                id = string.Format("L{0}_C{1}_P{2}_T{3}_Y{4}", t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId, t.TacticTypeId),
                text = t.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.StartDate,
                                                          t.EndDate),
                progress = GetTacticProgress(t, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
                open = false,
                parent = string.Format("L{0}_C{1}_P{2}", t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId),
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.TacticType.ColorCode.ToLower()),
                isSubmitted = t.Status.Equals(tacticStatusSubmitted),
                isDeclined = t.Status.Equals(tacticStatusDeclined),
                projectedStageValue = stageList.Single(s => s.StageId == t.StageId).Level <= inqLevel ? Convert.ToString(tacticStageRelationList.Single(tm => tm.TacticObj.PlanTacticId == t.PlanTacticId).INQValue) : "N/A",
                mqls = stageList.Single(s => s.StageId == t.StageId).Level <= mqlLevel ? Convert.ToString(tacticStageRelationList.Single(tm => tm.TacticObj.PlanTacticId == t.PlanTacticId).MQLValue) : "N/A",
                cost = t.Cost,
                cws = t.Status.Equals(tacticStatusSubmitted) || t.Status.Equals(tacticStatusDeclined) ? Math.Round(tacticStageRelationList.Single(tm => tm.TacticObj.PlanTacticId == t.PlanTacticId).RevenueValue, 1) : 0,
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
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          p.Plan_Campaign_Program.StartDate,
                                                          p.Plan_Campaign_Program.EndDate),
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
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          c.Plan_Campaign_Program.Plan_Campaign.StartDate,
                                                          c.Plan_Campaign_Program.Plan_Campaign.EndDate),
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
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          GetMinStartDateForPlan(GanttTabs.Tactic, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic),
                                                          GetMaxEndDateForPlan(GanttTabs.Tactic, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
                progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Tactic, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
                                                Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
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

            if (NewTaskDataTactic.Count() > 0)
            {
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
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                              CalendarEndDate,
                                                              ita.minStartDate,
                                                              CalendarEndDate) - 1,
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
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                              CalendarEndDate,
                                                              it.ImprovementTactic.EffectiveDate,
                                                              CalendarEndDate) - 1,
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

                //return taskDataCampaign.Concat<object>(taskDataTactic).Concat<object>(taskDataProgram).ToList<object>();
                return newTaskDataPlan.Concat<object>(taskDataImprovementActivity).Concat<object>(taskDataImprovementTactic).Concat<object>(newTaskDataCampaign).Concat<object>(NewTaskDataTactic).Concat<object>(newTaskDataProgram).ToList<object>();
            }
            else
            {
                return newTaskDataPlan.Concat<object>(newTaskDataCampaign).Concat<object>(NewTaskDataTactic).Concat<object>(newTaskDataProgram).ToList<object>();
            }
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
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/19/2013
        /// Function to get minimum date for vertical, audience task.
        /// </summary>
        /// <param name="type">Type of tab i.e. Vertical, Audience.</param>
        /// <param name="typeId">Type Id i.e. Vertical, Audience.</param>
        /// <param name="campaign">Campaign of plan version.</param>
        /// <param name="program">Program of plan version.</param>
        /// <param name="tactic">Tactic of plan version.</param>
        /// <returns>Returns minimum date for audience, vertical task for GANNT CHART.</returns>
        //public DateTime GetMinStartDate(GanttTabs currentGanttTab, int typeId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
        //{
        //    var queryPlanProgramId = new List<int>();

        //    DateTime minDateTactic = DateTime.Now;
        //    switch (currentGanttTab)
        //    {
        //        case GanttTabs.Vertical:
        //            queryPlanProgramId = tactic.Where(t => t.VerticalId == typeId).Select(t => t.PlanProgramId).ToList<int>();
        //            minDateTactic = tactic.Where(t => t.VerticalId == typeId).Select(t => t.StartDate).Min();
        //            break;
        //        case GanttTabs.Audience:
        //            queryPlanProgramId = tactic.Where(t => t.AudienceId == typeId).Select(t => t.PlanProgramId).ToList<int>();
        //            minDateTactic = tactic.Where(t => t.AudienceId == typeId).Select(t => t.StartDate).Min();
        //            break;
        //        default:
        //            break;
        //    }

        //    var queryPlanCampaignId = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.PlanCampaignId).ToList<int>();
        //    DateTime minDateProgram = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.StartDate).Min();

        //    DateTime minDateCampaign = campaign.Where(c => queryPlanCampaignId.Contains(c.PlanCampaignId)).Select(c => c.StartDate).Min();

        //    return new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
        //}

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
        public DateTime GetMinStartDateForCustomField(PlanGanttTypes currentGanttTab, int typeId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
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
                    queryPlanProgramId = tactic.Where(t => t.PlanTacticId == typeId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.PlanTacticId == typeId).Select(t => t.StartDate).Min();
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
                    minDateTactic = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.StartDate).Min();
                    break;
                case GanttTabs.BusinessUnit:
                    var businessUnitId = tactic.Where(t => t.PlanTacticId == typeId).Select(t => t.BusinessUnitId).FirstOrDefault();
                    queryPlanProgramId = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId && t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId == businessUnitId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId && t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId == businessUnitId).Select(t => t.StartDate).Min();
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
        public DateTime GetMinStartDateForPlanNew(string currentGanttTab, int typeId, int planId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
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
                    queryPlanProgramId = tactic.Where(t => t.PlanTacticId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.PlanTacticId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.StartDate).Min();
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
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/19/2013
        /// Function to get maximum date for vertical, audience task.
        /// </summary>
        /// <param name="type">Type of tab i.e. Vertical, Audience.</param>
        /// <param name="typeId">Type Id i.e. Vertical, Audience.</param>
        /// <param name="campaign">Campaign of plan version.</param>
        /// <param name="program">Program of plan version.</param>
        /// <param name="tactic">Tactic of plan version.</param>
        /// <returns>Returns maximum date for audience, vertical task for GANNT CHART.</returns>
        //public DateTime GetMaxEndDate(GanttTabs currentGanttTab, int typeId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
        //{
        //    var queryPlanProgramId = new List<int>();

        //    DateTime maxDateTactic = DateTime.Now;

        //    switch (currentGanttTab)
        //    {
        //        case GanttTabs.Vertical:
        //            queryPlanProgramId = tactic.Where(t => t.VerticalId == typeId).Select(t => t.PlanProgramId).ToList<int>();
        //            maxDateTactic = tactic.Where(t => t.VerticalId == typeId).Select(t => t.EndDate).Max();
        //            break;
        //        case GanttTabs.Audience:
        //            queryPlanProgramId = tactic.Where(t => t.AudienceId == typeId).Select(t => t.PlanProgramId).ToList<int>();
        //            maxDateTactic = tactic.Where(t => t.AudienceId == typeId).Select(t => t.EndDate).Max();
        //            break;
        //        default:
        //            break;
        //    }

        //    var queryPlanCampaignId = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.PlanCampaignId).ToList<int>();
        //    DateTime maxDateProgram = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.EndDate).Max();

        //    DateTime maxDateCampaign = campaign.Where(c => queryPlanCampaignId.Contains(c.PlanCampaignId)).Select(c => c.EndDate).Max();

        //    return new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
        //}

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
        public DateTime GetMaxEndDateForCustomField(PlanGanttTypes currentGanttTab, int typeId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
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
                    queryPlanProgramId = tactic.Where(t => t.PlanTacticId == typeId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.PlanTacticId == typeId).Select(t => t.EndDate).Max();
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
        public DateTime GetMaxEndDateForPlanNew(string currentGanttTab, int typeId, int planId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
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
                    queryPlanProgramId = tactic.Where(t => t.PlanTacticId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.PlanTacticId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.EndDate).Max();
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

        /// <summary>
        /// Added By: Maninder Singh.
        /// Modified By Maninder Singh Wadhva PL Ticket#47
        /// Action to Save Comment & Update Status as of selected tactic.
        /// </summary>
        /// <param name="planTacticId">Plan Tactic Id.</param>
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
                            if (isImprovement)
                            {
                                Plan_Improvement_Campaign_Program_Tactic improvementPlanTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => improvementTactic.ImprovementPlanTacticId.Equals(planTacticId)).SingleOrDefault();
                                improvementPlanTactic.Status = status;
                                improvementPlanTactic.ModifiedBy = Sessions.User.UserId;
                                improvementPlanTactic.ModifiedDate = DateTime.Now;

                                Plan_Improvement_Campaign_Program_Tactic_Comment improvementPlanTacticComment = new Plan_Improvement_Campaign_Program_Tactic_Comment()
                                {
                                    ImprovementPlanTacticId = planTacticId,
                                    //changes done from displayname to FName and Lname by uday for internal issue on 27-6-2014
                                    Comment = string.Format("Improvement Tactic {0} by {1}", status, Sessions.User.FirstName + " " + Sessions.User.LastName),
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = Sessions.User.UserId
                                };

                                db.Entry(improvementPlanTacticComment).State = EntityState.Added;
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
                                            ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, new Guid(), EntityType.ImprovementTactic);
                                            externalIntegration.Sync();
                                        }
                                        //added by uday for #532
                                    }
                                    else if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                    {
                                        result = Common.InsertChangeLog(improvementPlanTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, 0, planTacticId, improvementPlanTactic.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined);
                                    }
                                    Common.mailSendForTactic(planTacticId, status, improvementPlanTactic.Title, section: Convert.ToString(Enums.Section.ImprovementTactic).ToLower());
                                    if (result >= 1)
                                    {
                                        scope.Complete();
                                        return Json(new { result = true }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            else
                            {
                                Plan_Campaign_Program_Tactic tactic = db.Plan_Campaign_Program_Tactic.Where(pt => pt.PlanTacticId == planTacticId).SingleOrDefault();
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
                                            ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, new Guid(), EntityType.Tactic);
                                            externalIntegration.Sync();
                                        }
                                        //// End by uday for #532
                                    }
                                    else if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                    {
                                        result = Common.InsertChangeLog(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, 0, planTacticId, tactic.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined);
                                    }
                                    Common.mailSendForTactic(planTacticId, status, tactic.Title, section: Convert.ToString(Enums.Section.Tactic).ToLower());

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
            }

            return null;
        }
        #endregion

        #region "Inspect"

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Inspect Popup.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="section">Decide which section to open for Inspect Popup (tactic,program or campaign)</param>
        /// <param name="TabValue">Tab value of Popup.</param>
        /// <returns>Returns Partial View Of Inspect Popup.</returns>
        public ActionResult LoadInspectPopup(int id, string section, string TabValue = "Setup")
        {
            try
            {
                // To get permission status for Add/Edit Actual, By dharmraj PL #519
                ViewBag.IsTacticActualsAddEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);
                //Get all subordinates of current user upto n level
                var lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
                // Get current user permission for edit own and subordinates plans.
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);

                //// Added by Pratik Chauhan for functional review points

                Plan_Campaign_Program_Tactic objPlan_Campaign_Program_Tactic = null;
                Plan_Campaign_Program objPlan_Campaign_Program = null;
                Plan_Campaign objPlan_Campaign = null;
                Plan_Improvement_Campaign_Program_Tactic objPlan_Improvement_Campaign_Program_Tactic = null;
                int planId = 0;

                if (Convert.ToString(section) != "")
                {
                    if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        objPlan_Campaign_Program_Tactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(id)).FirstOrDefault();
                        planId = db.Plan_Campaign.Where(pcobjw => pcobjw.PlanCampaignId.Equals(db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(objPlan_Campaign_Program_Tactic.PlanProgramId)).Select(r => r.PlanCampaignId).FirstOrDefault())).Select(r => r.PlanId).FirstOrDefault();
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        objPlan_Campaign_Program = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(id)).FirstOrDefault();
                        planId = db.Plan_Campaign.Where(pcpobjw => pcpobjw.PlanCampaignId.Equals(objPlan_Campaign_Program.PlanCampaignId)).Select(r => r.PlanId).FirstOrDefault();
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        objPlan_Campaign = db.Plan_Campaign.Where(pcpobjw => pcpobjw.PlanCampaignId.Equals(id)).FirstOrDefault();
                        planId = objPlan_Campaign.PlanId;
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        objPlan_Improvement_Campaign_Program_Tactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(picpobjw => picpobjw.ImprovementPlanTacticId.Equals(id)).FirstOrDefault();
                        planId = db.Plan_Improvement_Campaign.Where(picobjw => picobjw.ImprovementPlanCampaignId.Equals(db.Plan_Improvement_Campaign_Program.Where(picpobjw => picpobjw.ImprovementPlanProgramId.Equals(objPlan_Improvement_Campaign_Program_Tactic.ImprovementPlanProgramId)).Select(r => r.ImprovementPlanCampaignId).FirstOrDefault())).Select(r => r.ImprovePlanId).FirstOrDefault();
                    }
                }

                var objPlan = db.Plans.FirstOrDefault(p => p.PlanId == planId);
                bool IsPlanEditable = false;
                bool IsBusinessUnitEditable = Common.IsBusinessUnitEditable(Sessions.BusinessUnitId);   // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                if (IsBusinessUnitEditable)
                {
                    if (objPlan.CreatedBy.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
                    {
                        IsPlanEditable = true;
                    }
                    else if (IsPlanEditAllAuthorized) // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                    {
                        IsPlanEditable = true;
                    }
                    else if (IsPlanEditSubordinatesAuthorized)
                    {
                        if (lstOwnAndSubOrdinates.Contains(objPlan.CreatedBy)) // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                        {
                            IsPlanEditable = true;
                        }
                    }
                }

                if (Convert.ToString(section) != "")
                {
                    string verticalId = string.Empty;
                    string GeographyId = string.Empty;
                    if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        //Plan_Campaign_Program_Tactic objPlan_Campaign_Program_Tactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(id)).SingleOrDefault();
                        verticalId = objPlan_Campaign_Program_Tactic.VerticalId.ToString();
                        GeographyId = objPlan_Campaign_Program_Tactic.GeographyId.ToString();
                        DateTime todaydate = DateTime.Now;
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
                        //Plan_Campaign_Program objPlan_Campaign_Program = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(id)).SingleOrDefault();
                        //verticalId = objPlan_Campaign_Program.VerticalId == null ? string.Empty : objPlan_Campaign_Program.VerticalId.ToString();
                        GeographyId = objPlan_Campaign_Program.GeographyId == null ? string.Empty : objPlan_Campaign_Program.GeographyId.ToString();
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
                        //Plan_Campaign objPlan_Campaign = db.Plan_Campaign.Where(pcpobjw => pcpobjw.PlanCampaignId.Equals(id)).SingleOrDefault();
                        //verticalId = objPlan_Campaign.VerticalId == null ? string.Empty : objPlan_Campaign.VerticalId.ToString();
                        GeographyId = objPlan_Campaign.GeographyId == null ? string.Empty : objPlan_Campaign.GeographyId.ToString();
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

                    //47.	Check only for tactic, bhavesh internal review point, modified by Dharmraj
                    if (IsPlanEditable && Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        // Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
                        var lstUserCustomRestriction = Common.GetUserCustomRestriction();
                        int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                        var lstAllowedVertical = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId).ToList();
                        var lstAllowedGeography = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId.ToString().ToLower()).ToList();////Modified by Mitesh Vaishnav For functional review point 89

                        if (GeographyId != string.Empty && verticalId != string.Empty)
                        {
                            if (lstAllowedGeography.Contains(GeographyId.ToLower()) && lstAllowedVertical.Contains(verticalId))////Modified by Mitesh Vaishnav For functional review point 89
                            {
                                IsPlanEditable = true;
                            }
                            else
                            {
                                IsPlanEditable = false;
                            }
                        }
                    }
                }

                ViewBag.IsPlanEditable = IsPlanEditable;
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
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
                return PartialView("_InspectPopupImprovementTactic", im);
            }
            return PartialView("InspectPopup", im);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Setup Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadSetup(int id)
        {
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.Tactic).ToLower(), false);     //// Modified by :- Sohel Pathan on 27/05/2014 for PL ticket #425
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }

            }
            im.Owner = (userName.FirstName + " " + userName.LastName).ToString();
            ViewBag.TacticDetail = im;

            ViewBag.BudinessUnitTitle = db.BusinessUnits.Where(b => b.BusinessUnitId == im.BusinessUnitId && b.IsDeleted == false).Select(b => b.Title).SingleOrDefault();//Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
            ViewBag.Audience = db.Audiences.Where(a => a.AudienceId == im.AudienceId).Select(a => a.Title).SingleOrDefault();

            return PartialView("SetUp", im);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Review Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadReview(int id)
        {
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.Tactic).ToLower(), false);     //// Modified by :- Sohel Pathan on 27/05/2014 for PL ticket #425
            var tacticComment = (from tc in db.Plan_Campaign_Program_Tactic_Comment
                                 where tc.PlanTacticId == id
                                 select tc).ToArray();
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
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

            var ownername = (from u in userName
                             where u.UserId == im.OwnerId
                             select u.FirstName + " " + u.LastName).FirstOrDefault();
            if (ownername != null)
            {
                im.Owner = ownername.ToString();
            }

            string TitleMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            int MQLStageLevel = Convert.ToInt32(db.Stages.FirstOrDefault(s => s.ClientId == Sessions.User.ClientId && s.Code == TitleMQL).Level);
            //Compareing MQL stage level with tactic stage level
            if (im.StageLevel < MQLStageLevel)
            {
                ViewBag.ShowMQL = true;
                im.MQLs = Common.CalculateMQLTactic(Convert.ToDouble(im.ProjectedStageValue), im.StartDate, im.PlanTacticId, Convert.ToInt32(im.StageId));
            }
            else
            {
                ViewBag.ShowMQL = false;
            }

            ViewBag.TacticDetail = im;
            ViewBag.IsModelDeploy = im.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690

            var businessunittitle = (from bun in db.BusinessUnits
                                     where bun.BusinessUnitId == im.BusinessUnitId
                                     select bun.Title).FirstOrDefault();
            ViewBag.BudinessUnitTitle = businessunittitle.ToString();

            bool isValidOwner = false;
            if (im.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
            }
            ViewBag.IsValidOwner = isValidOwner;
            /*Added by Mitesh Vaishnav on 13/06/2014 to address changes related to #498 Customized Target Stage - Publish model*/
            var pcpt = db.Plan_Campaign_Program_Tactic.Where(p => p.PlanTacticId.Equals(id)).SingleOrDefault();
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
            var lstSubOrdinatesPeers = Common.GetSubOrdinatesWithPeersNLevel();
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

            // Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
            var lstUserCustomRestriction = Common.GetUserCustomRestriction();
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            var lstAllowedVertical = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId).ToList();
            var lstAllowedGeography = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId.ToString().ToLower()).ToList();////Modified by Mitesh Vaishnav For functional review point 89
            bool IsTacticEditable = false;
            if (lstAllowedGeography.Contains(pcpt.GeographyId.ToString().ToLower()) && lstAllowedVertical.Contains(pcpt.VerticalId.ToString()))////Modified by Mitesh Vaishnav For functional review point 89
            {
                IsTacticEditable = true;
            }
            else
            {
                IsTacticEditable = false;
            }

            ViewBag.IsTacticEditable = IsTacticEditable;

            // Start - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            bool IsBusinessUnitEditable = Common.IsBusinessUnitEditable(Sessions.BusinessUnitId);
            if (IsBusinessUnitEditable)
                ViewBag.IsBusinessUnitEditable = true;
            else
                ViewBag.IsBusinessUnitEditable = false;
            // End - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units

            // Added by Dharmraj Mangukiya for Deploy to integration button restrictions PL ticket #537
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
            bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            //Get all subordinates of current user upto n level
            var lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            bool IsDeployToIntegrationVisible = false;
            if (IsBusinessUnitEditable)
            {
                if (im.OwnerId.Equals(Sessions.User.UserId) && IsTacticEditable) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
                {
                    IsDeployToIntegrationVisible = true;
                }
                else if (IsPlanEditAllAuthorized && IsTacticEditable)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsDeployToIntegrationVisible = true;
                }
                else if (IsPlanEditSubordinatesAuthorized)
                {
                    if (lstSubOrdinates.Contains(im.OwnerId))
                    {
                        if (IsTacticEditable)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                        {
                            IsDeployToIntegrationVisible = true;
                        }
                    }
                }
            }

            ViewBag.IsDeployToIntegrationVisible = IsDeployToIntegrationVisible;
            ////Start : Added by Mitesh Vaishnav for PL ticket #690 Model Interface - Integration
            ViewBag.TacticIntegrationInstance = db.Plan_Campaign_Program_Tactic.Where(ti => ti.PlanTacticId == im.PlanTacticId).FirstOrDefault().IntegrationInstanceTacticId;
            string pullResponses = Operation.Pull_Responses.ToString();
            string pullClosedWon = Operation.Pull_ClosedWon.ToString();
            string pullQualifiedLeads = Operation.Pull_QualifiedLeads.ToString();
            string planEntityType = Enums.Section.Tactic.ToString();
            var planEntityLogList = db.IntegrationInstancePlanEntityLogs.Where(ipt => ipt.EntityId == im.PlanTacticId && ipt.EntityType == planEntityType).OrderByDescending(ipt => ipt.IntegrationInstancePlanLogEntityId).ToList();
            if (planEntityLogList.Where(p => p.Operation == pullResponses).FirstOrDefault() != null)
            {
                // viewbag which display last synced datetime of tactic for pull responses
                ViewBag.INQLastSync = planEntityLogList.Where(p => p.Operation == pullResponses).FirstOrDefault().SyncTimeStamp;
            }
            if (planEntityLogList.Where(p => p.Operation == pullClosedWon).FirstOrDefault() != null)
            {
                // viewbag which display last synced datetime of tactic for pull closed won
                ViewBag.CWLastSync = planEntityLogList.Where(p => p.Operation == pullClosedWon).FirstOrDefault().SyncTimeStamp;
            }
            if (planEntityLogList.Where(p => p.Operation == pullQualifiedLeads).FirstOrDefault() != null)
            {
                // viewbag which display last synced datetime of tactic for pull qualified leads
                ViewBag.MQLLastSync = planEntityLogList.Where(p => p.Operation == pullQualifiedLeads).FirstOrDefault().SyncTimeStamp;
            }
            ////End : Added by Mitesh Vaishnav for PL ticket #690 Model Interface - Integration
            return PartialView("Review");
        }

        public JsonResult SaveSyncToIntegration(int id, string section, bool IsDeployedToIntegration)
        {
            bool returnValue = false;

            try
            {
                if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                {
                    var objTactic = db.Plan_Campaign_Program_Tactic.SingleOrDefault(varT => varT.PlanTacticId == id);
                    objTactic.IsDeployedToIntegration = IsDeployedToIntegration;
                    db.Entry(objTactic).State = EntityState.Modified;
                    db.SaveChanges();
                    returnValue = true;
                }
                else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                {
                    var objProgram = db.Plan_Campaign_Program.SingleOrDefault(varT => varT.PlanProgramId == id);
                    objProgram.IsDeployedToIntegration = IsDeployedToIntegration;
                    db.Entry(objProgram).State = EntityState.Modified;
                    db.SaveChanges();
                    returnValue = true;
                }
                else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                {
                    var objCampaign = db.Plan_Campaign.SingleOrDefault(varT => varT.PlanCampaignId == id);
                    objCampaign.IsDeployedToIntegration = IsDeployedToIntegration;
                    db.Entry(objCampaign).State = EntityState.Modified;
                    db.SaveChanges();
                    returnValue = true;
                }
                else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                {
                    var objITactic = db.Plan_Improvement_Campaign_Program_Tactic.SingleOrDefault(varT => varT.ImprovementPlanTacticId == id);
                    objITactic.IsDeployedToIntegration = IsDeployedToIntegration;
                    db.Entry(objITactic).State = EntityState.Modified;
                    db.SaveChanges();
                    returnValue = true;
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { result = returnValue, msg = "Result Updated Successfully" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get InspectModel.
        /// </summary>
        /// Modifled by :- Sohel Pathan on 27/05/2014 for PL ticket #425, Default parameter added named isStatusChange
        /// <param name="id">Plan Tactic Id.</param>
        /// <param section="id">.Decide which section to open for Inspect Popup (tactic,program or campaign)</param>
        /// <returns>Returns InspectModel.</returns>
        private InspectModel GetInspectModel(int id, string section, bool isStatusChange = true)
        {

            InspectModel imodel = new InspectModel();
            string statusapproved = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Approved.ToString()].ToString();
            string statusinprogress = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.InProgress.ToString()].ToString();
            string statuscomplete = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Complete.ToString()].ToString();
            string statusdecline = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Decline.ToString()].ToString();
            string statussubmit = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Submitted.ToString()].ToString();

            try
            {
                if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                {
                    imodel = (from pcpt in db.Plan_Campaign_Program_Tactic
                              where pcpt.PlanTacticId == id && pcpt.IsDeleted == false
                              select new InspectModel
                              {
                                  PlanTacticId = pcpt.PlanTacticId,
                                  TacticTitle = pcpt.Title,
                                  TacticTypeTitle = pcpt.TacticType.Title,
                                  CampaignTitle = pcpt.Plan_Campaign_Program.Plan_Campaign.Title,
                                  ProgramTitle = pcpt.Plan_Campaign_Program.Title,
                                  Status = pcpt.Status,
                                  TacticTypeId = pcpt.TacticTypeId,
                                  VerticalId = pcpt.VerticalId,
                                  ColorCode = pcpt.TacticType.ColorCode,
                                  Description = pcpt.Description,
                                  AudienceId = pcpt.AudienceId,
                                  PlanCampaignId = pcpt.Plan_Campaign_Program.PlanCampaignId,
                                  PlanProgramId = pcpt.PlanProgramId,
                                  OwnerId = pcpt.CreatedBy,
                                  BusinessUnitId = pcpt.BusinessUnitId,
                                  //Modified By : Kalpesh Sharma #864 Add Actuals: Unable to update actuals % 864_Actuals.jpg %
                                  // If tactic has a line item at that time we have consider Project cost as sum of all the active line items
                                  Cost = (pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == pcpt.PlanTacticId && s.IsDeleted == false)).Count() > 0 ?
                                            (pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == pcpt.PlanTacticId && s.IsDeleted == false)).Sum(a => a.Cost) : pcpt.Cost,
                                  StartDate = pcpt.StartDate,
                                  EndDate = pcpt.EndDate,
                                  VerticalTitle = pcpt.Vertical.Title,
                                  AudiencTitle = pcpt.Audience.Title,
                                  CostActual = pcpt.CostActual == null ? 0 : pcpt.CostActual,
                                  IsDeployedToIntegration = pcpt.IsDeployedToIntegration,
                                  LastSyncDate = pcpt.LastSyncDate,
                                  StageId = pcpt.StageId,
                                  StageTitle = pcpt.Stage.Title,
                                  StageLevel = pcpt.Stage.Level,
                                  ProjectedStageValue = pcpt.ProjectedStageValue,
                                  GeographyTitle = pcpt.Geography.Title

                              }).SingleOrDefault();

                    imodel.IsIntegrationInstanceExist = CheckIntegrationInstanceExist(db.Plan_Campaign_Program_Tactic.SingleOrDefault(varT => varT.PlanTacticId == id).TacticType.Model);
                }
                if (section == Convert.ToString(Enums.Section.Program).ToLower())
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
                    imodel.VerticalId = objPlan_Campaign_Program.VerticalId;
                    imodel.ColorCode = Program_InspectPopup_Flag_Color;
                    imodel.Description = objPlan_Campaign_Program.Description;
                    imodel.AudienceId = objPlan_Campaign_Program.AudienceId;
                    imodel.PlanCampaignId = objPlan_Campaign_Program.PlanCampaignId;
                    imodel.PlanProgramId = objPlan_Campaign_Program.PlanProgramId;
                    imodel.OwnerId = objPlan_Campaign_Program.CreatedBy;
                    imodel.BusinessUnitId = objPlan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId;
                    imodel.Cost = Common.CalculateProgramCost(objPlan_Campaign_Program.PlanProgramId); //objPlan_Campaign_Program.Cost; // Modified for PL#440 by Dharmraj
                    imodel.StartDate = objPlan_Campaign_Program.StartDate;
                    imodel.EndDate = objPlan_Campaign_Program.EndDate;

                    imodel.IsDeployedToIntegration = objPlan_Campaign_Program.IsDeployedToIntegration;
                    imodel.LastSyncDate = objPlan_Campaign_Program.LastSyncDate;

                    imodel.IsIntegrationInstanceExist = CheckIntegrationInstanceExist(objPlan_Campaign_Program.Plan_Campaign.Plan.Model);
                }

                if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
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
                    if (objPlan_Campaign.VerticalId != null)
                        imodel.VerticalId = objPlan_Campaign.VerticalId;
                    imodel.ColorCode = Campaign_InspectPopup_Flag_Color;
                    imodel.Description = objPlan_Campaign.Description;
                    if (objPlan_Campaign.AudienceId != null)
                        imodel.AudienceId = objPlan_Campaign.AudienceId;
                    imodel.PlanCampaignId = objPlan_Campaign.PlanCampaignId;
                    imodel.OwnerId = objPlan_Campaign.CreatedBy;
                    imodel.BusinessUnitId = objPlan_Campaign.Plan.Model.BusinessUnitId;
                    imodel.Cost = Common.CalculateCampaignCost(objPlan_Campaign.PlanCampaignId); //objPlan_Campaign.Cost; // Modified for PL#440 by Dharmraj
                    imodel.StartDate = objPlan_Campaign.StartDate;
                    imodel.EndDate = objPlan_Campaign.EndDate;

                    imodel.IsDeployedToIntegration = objPlan_Campaign.IsDeployedToIntegration;
                    imodel.IsIntegrationInstanceExist = CheckIntegrationInstanceExist(objPlan_Campaign.Plan.Model);
                    imodel.LastSyncDate = objPlan_Campaign.LastSyncDate;

                }

                if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
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
                                  BusinessUnitId = pcpt.BusinessUnitId,
                                  Cost = pcpt.Cost,
                                  StartDate = pcpt.EffectiveDate,
                                  IsDeployedToIntegration = pcpt.IsDeployedToIntegration,
                                  LastSyncDate = pcpt.LastSyncDate
                              }).SingleOrDefault();

                    imodel.IsIntegrationInstanceExist = CheckIntegrationInstanceExist(db.Plan_Improvement_Campaign_Program_Tactic.SingleOrDefault(varT => varT.ImprovementPlanTacticId == id).Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model);

                }

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return imodel;
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

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Actuals Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <returns>Returns Partial View Of Actuals Tab.</returns>
        public ActionResult LoadActuals(int id)
        {
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.Tactic).ToLower(), false);     //// Modified by :- Sohel Pathan on 27/05/2014 for PL ticket #425
            ViewBag.TacticStageId = im.StageId;
            ViewBag.TacticStageTitle = im.StageTitle;

            List<string> lstStageTitle = new List<string>();
            lstStageTitle = Common.GetTacticStageTitle(id);

            //string[] aryStageTitle = new string[] { Enums.InspectStageValues[Enums.InspectStage.INQ.ToString()].ToString(), Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString(), Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString(), Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString() };
            //ViewBag.StageTitle = aryStageTitle;

            ViewBag.StageTitle = lstStageTitle;


            List<Plan_Campaign_Program_Tactic> tid = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanTacticId == id).ToList();
            List<ProjectedRevenueClass> tacticList = Common.ProjectedRevenueCalculateList(tid);
            im.Revenues = Math.Round(tacticList.Where(tl => tl.PlanTacticId == id).Select(tl => tl.ProjectedRevenue).SingleOrDefault(), 2); // Modified by Sohel Pathan on 15/09/2014 for PL ticket #760
            tacticList = Common.ProjectedRevenueCalculateList(tid, true);

            string TitleProjectedStageValue = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
            string TitleCW = Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString();
            string TitleMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            string TitleRevenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();

            ////Modified by Mitesh Vaishnav for PL ticket #695
            var tacticActualList = db.Plan_Campaign_Program_Tactic_Actual.Where(planTacticActuals => planTacticActuals.PlanTacticId == id).ToList();
            im.ProjectedStageValueActual = tacticActualList.Where(planTacticActuals => planTacticActuals.StageTitle == TitleProjectedStageValue).Sum(planTacticActuals => planTacticActuals.Actualvalue);
            im.CWsActual = tacticActualList.Where(planTacticActuals => planTacticActuals.StageTitle == TitleCW).Sum(planTacticActuals => planTacticActuals.Actualvalue);
            im.RevenuesActual = tacticActualList.Where(planTacticActuals => planTacticActuals.StageTitle == TitleRevenue).Sum(planTacticActuals => planTacticActuals.Actualvalue);
            im.MQLsActual = tacticActualList.Where(planTacticActuals => planTacticActuals.StageTitle == TitleMQL).Sum(planTacticActuals => planTacticActuals.Actualvalue);


            im.CWs = Math.Round(tacticList.Where(tl => tl.PlanTacticId == id).Select(tl => tl.ProjectedRevenue).SingleOrDefault(), 1);
            string modifiedBy = string.Empty;
            modifiedBy = Common.TacticModificationMessage(im.PlanTacticId);////Modified by Mitesh Vaishnav for PL ticket #743 Actuals Inspect: User Name for Scheduler Integration
            ViewBag.UpdatedBy = modifiedBy != string.Empty ? modifiedBy : null;////Modified by Mitesh Vaishnav for PL ticket #743 Actuals Inspect: User Name for Scheduler Integration
            im.MQLs = Common.CalculateMQLTactic(Convert.ToDouble(im.ProjectedStageValue), im.StartDate, im.PlanTacticId, Convert.ToInt32(im.StageId));

            // Modified by dharmraj for implement new formula to calculate ROI, #533
            if (im.Cost > 0)
            {
                im.ROI = (im.Revenues - im.Cost) / im.Cost;
            }
            else
                im.ROI = 0;
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
                var tacticActualCostList = tacticActualList.Where(tacticActual => tacticActual.StageTitle == costStageTitle)
                                                                            .ToList();
                if (tacticActualCostList.Count != 0)
                {
                    tacticCostActual = tacticActualCostList.Sum(tacticActual => tacticActual.Actualvalue);
                }
            }
            if (tacticCostActual > 0)
            {
                im.ROIActual = (im.RevenuesActual - tacticCostActual) / tacticCostActual;
            }
            else
            {
                im.ROIActual = 0;
            }
            ////End Modified by Mitesh Vaishnav For PL ticket #695
            ViewBag.TacticDetail = im;
            bool isValidUser = true;
            //if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            //{
            if (im.OwnerId != Sessions.User.UserId) isValidUser = false;
            //}
            ViewBag.IsValidUser = isValidUser;

            ViewBag.IsModelDeploy = im.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690
            if (im.LastSyncDate != null)
            {
                ViewBag.LastSync = "Last synced with integration " + Common.GetFormatedDate(im.LastSyncDate) + ".";////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            }
            else
            {
                ViewBag.LastSync = string.Empty;
            }

            // Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
            var lstUserCustomRestriction = Common.GetUserCustomRestriction();
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            var lstAllowedVertical = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId).ToList();
            var lstAllowedGeography = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId.ToString().ToLower()).ToList();////Modified by Mitesh Vaishnav For functional review point 89
            var lstAllowedBusinessUnit = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).Select(r => r.CustomFieldId.ToLower()).ToList();////Modified by Mitesh Vaishnav For functional review point 89
            //var IsBusinessUnitEditable = Common.IsBusinessUnitEditable(Sessions.BusinessUnitId);    // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            if (lstAllowedGeography.Contains(tid.FirstOrDefault().GeographyId.ToString().ToLower()) && lstAllowedVertical.Contains(tid.FirstOrDefault().VerticalId.ToString()) && lstAllowedBusinessUnit.Contains(tid.FirstOrDefault().BusinessUnitId.ToString().ToLower()))//&& IsBusinessUnitEditable)   // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                if (Common.CheckAfterApprovedStatus(im.Status))
                {
                    ViewBag.IsTacticEditable = true;
                }
                else
                {
                    ViewBag.IsTacticEditable = false;
                }
            }
            else
            {
                ViewBag.IsTacticEditable = false;
            }
            ViewBag.LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == id && l.IsDeleted == false).OrderByDescending(l => l.LineItemTypeId).ToList();
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
            var dtTactic = (from pt in db.Plan_Campaign_Program_Tactic_Actual
                            where pt.PlanTacticId == id
                            select new { pt.CreatedBy, pt.CreatedDate }).FirstOrDefault();
            var lineItemIds = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == id && l.IsDeleted == false).Select(l => l.PlanLineItemId).ToList();
            var dtlineItemActuals = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(al => lineItemIds.Contains(al.PlanLineItemId)).ToList();
            if (dtTactic != null || dtlineItemActuals != null)
            {
                //User userName = objBDSUserRepository.GetTeamMemberDetails(dtTactic.CreatedBy, Sessions.ApplicationId);
                //string lstUpdate = string.Format("{0} {1} by {2} {3}", "Last updated", dtTactic.CreatedDate.ToString("MMM dd"), userName.FirstName, userName.LastName);
                var ActualData = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpta => pcpta.PlanTacticId == id).Select(pt => new
                {
                    id = pt.PlanTacticId,
                    stageTitle = pt.StageTitle,
                    period = pt.Period,
                    actualValue = pt.Actualvalue
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
        /// <returns>Returns JsonResult.</returns>
        [HttpPost]
        public JsonResult UploadResult(List<InspectActual> tacticactual, List<Plan_Campaign_Program_Tactic_LineItem_Actual> lineItemActual, string UserId = "")
        {
            bool isLineItemForTactic = false;
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
                    var actualResult = (from t in tacticactual
                                        select new { t.PlanTacticId, t.TotalProjectedStageValueActual, t.TotalMQLActual, t.TotalCWActual, t.TotalRevenueActual, t.TotalCostActual, t.ROI, t.ROIActual, t.IsActual }).FirstOrDefault();
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

                            Plan_Campaign_Program_Tactic objPCPT = db.Plan_Campaign_Program_Tactic.Where(pt => pt.PlanTacticId == actualResult.PlanTacticId).SingleOrDefault();
                            //objPCPT.CostActual = actualResult.TotalCostActual;
                            objPCPT.ModifiedBy = Sessions.User.UserId;
                            objPCPT.ModifiedDate = DateTime.Now;
                            db.Entry(objPCPT).State = EntityState.Modified;
                            int result = db.SaveChanges();
                            result = Common.InsertChangeLog(Sessions.PlanId, null, actualResult.PlanTacticId, objPCPT.Title, Enums.ChangeLog_ComponentType.tacticresults, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                            scope.Complete();
                            return Json(new { id = actualResult.PlanTacticId, TabValue = "Actuals", msg = "Result Updated Successfully." });
                        }
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
        /// Save Line item Actual 
        /// Added By : Kalpesh Sharma #735 Actual cost - Changes to add actuals screen 
        /// Save the data into the Plan_Campaign_Program_Tactic_LineItem_Actual
        /// </summary>
        /// <param name="objInspectActual"></param>
        /// <param name="Id"></param>
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
                string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
                Regex r = new Regex(regex, RegexOptions.IgnoreCase);
                comment = r.Replace(comment, "<a href=\"$1\" title=\"Click to open in a new window or tab\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"http://www");



                ////End Added by Mitesh Vaishnav on 07/07/2014 for PL ticket #569: make urls in tactic notes hyperlinks
                if (ModelState.IsValid)
                {
                    if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
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
                        if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                        {
                            int PlanId = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == planTacticId).Select(p => p.Plan_Campaign_Program.Plan_Campaign.PlanId).FirstOrDefault();
                            Plan_Campaign_Program_Tactic pct = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanTacticId == planTacticId).SingleOrDefault();
                            Common.mailSendForTactic(planTacticId, Enums.Custom_Notification.TacticCommentAdded.ToString(), pct.Title, true, comment, Convert.ToString(Enums.Section.Tactic).ToLower(), Url.Action("Index", "Home", new { currentPlanId = PlanId, planTacticId = planTacticId }, Request.Url.Scheme));
                        }
                        else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                        {
                            Plan_Campaign_Program pcp = db.Plan_Campaign_Program.Where(t => t.PlanProgramId == planTacticId).SingleOrDefault();
                            Common.mailSendForTactic(planTacticId, Enums.Custom_Notification.ProgramCommentAdded.ToString(), pcp.Title, true, comment, Convert.ToString(Enums.Section.Program).ToLower());
                        }
                        else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                        {
                            Plan_Campaign pc = db.Plan_Campaign.Where(t => t.PlanCampaignId == planTacticId).SingleOrDefault();
                            Common.mailSendForTactic(planTacticId, Enums.Custom_Notification.CampaignCommentAdded.ToString(), pc.Title, true, comment, Convert.ToString(Enums.Section.Campaign).ToLower());
                        }
                        else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                        {
                            Plan_Improvement_Campaign_Program_Tactic pc = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.ImprovementPlanTacticId == planTacticId).SingleOrDefault();
                            Common.mailSendForTactic(planTacticId, Enums.Custom_Notification.ImprovementTacticCommentAdded.ToString(), pc.Title, true, comment, Convert.ToString(Enums.Section.ImprovementTactic).ToLower());
                        }
                    }

                    return Json(new { id = planTacticId, TabValue = "Review", msg = Common.objCached.CommentSuccessfully });
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
                                //approvedComment = "Tactic " + status + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
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
                                if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                                {
                                    Plan_Campaign_Program_Tactic tactic = db.Plan_Campaign_Program_Tactic.Where(pt => pt.PlanTacticId == planTacticId).SingleOrDefault();
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
                                                ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, new Guid(), EntityType.Tactic);
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
                                        Common.mailSendForTactic(planTacticId, status, tactic.Title, false, "", Convert.ToString(Enums.Section.Tactic).ToLower());
                                    }
                                    strmessage = Common.objCached.TacticStatusSuccessfully.Replace("{0}", status);

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
                                    Plan_Improvement_Campaign_Program_Tactic tactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(pt => pt.ImprovementPlanTacticId == planTacticId).SingleOrDefault();
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
                                                ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, new Guid(), EntityType.ImprovementTactic);
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
                                        Common.mailSendForTactic(planTacticId, status, tactic.Title, false, "", Convert.ToString(Enums.Section.ImprovementTactic).ToLower());
                                    }
                                    strmessage = Common.objCached.ImprovementTacticStatusSuccessfully.Replace("{0}", status);
                                }
                                else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                                {
                                    Plan_Campaign_Program program = db.Plan_Campaign_Program.Where(pt => pt.PlanProgramId == planTacticId).SingleOrDefault();
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
                                        Common.mailSendForTactic(planTacticId, status, program.Title, false, "", Convert.ToString(Enums.Section.Program).ToLower());
                                    }
                                    strmessage = Common.objCached.ProgramStatusSuccessfully.Replace("{0}", status);
                                }
                                else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                                {
                                    Plan_Campaign campaign = db.Plan_Campaign.Where(pt => pt.PlanCampaignId == planTacticId).SingleOrDefault();
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
                                        Common.mailSendForTactic(planTacticId, status, campaign.Title, false, "", Convert.ToString(Enums.Section.Campaign).ToLower());
                                    }
                                    strmessage = Common.objCached.CampaignStatusSuccessfully.Replace("{0}", status);
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
            }
            return Json(new { id = 0 });
        }

        /// <summary>
        /// Added By: Kunal.
        /// Action to Load Program Setup Tab.
        /// </summary>
        /// <param name="id">Plan Program Id.</param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadSetupProgram(int id)
        {
            InspectModel im = GetInspectModel(id, "program", false);        //// Modified by :- Sohel Pathan on 27/05/2014 for PL ticket #425
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
            }
            im.Owner = (userName.FirstName + " " + userName.LastName).ToString();
            ViewBag.ProgramDetail = im;

            if (im.LastSyncDate != null)
            {
                TimeZone localZone = TimeZone.CurrentTimeZone;

                ViewBag.LastSync = "Last synced with integration " + Common.GetFormatedDate(im.LastSyncDate) + ".";////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            }
            else
            {
                ViewBag.LastSync = string.Empty;
            }

            ViewBag.BudinessUnitTitle = db.BusinessUnits.Where(b => b.BusinessUnitId == im.BusinessUnitId && b.IsDeleted == false).Select(b => b.Title).SingleOrDefault();//Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
            ViewBag.Audience = db.Audiences.Where(a => a.AudienceId == im.AudienceId).Select(a => a.Title).SingleOrDefault();

            return PartialView("_SetupProgram", im);
        }

        /// <summary>
        /// Added By: Kunal.
        /// Action to Load Program Review Tab.
        /// </summary>
        /// <param name="id">Plan Program Id.</param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadReviewProgram(int id)
        {
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.Program).ToLower(), false);    //// Modified by :- Sohel Pathan on 27/05/2014 for PL ticket #425
            var tacticComment = (from tc in db.Plan_Campaign_Program_Tactic_Comment
                                 where tc.PlanProgramId == id && tc.PlanProgramId.HasValue
                                 select tc).ToArray();
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
            }

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
            ViewBag.ProgramDetail = im;

            var businessunittitle = (from bun in db.BusinessUnits
                                     where bun.BusinessUnitId == im.BusinessUnitId
                                     select bun.Title).FirstOrDefault();
            ViewBag.BudinessUnitTitle = businessunittitle.ToString();

            bool isValidOwner = false;
            if (im.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
            }
            ViewBag.IsValidOwner = isValidOwner;

            ViewBag.IsModelDeploy = im.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690

            //To get permission status for Approve campaign , By dharmraj PL #538
            var lstSubOrdinatesPeers = Common.GetSubOrdinatesWithPeersNLevel();

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

            // Start - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            bool IsBusinessUnitEditable = Common.IsBusinessUnitEditable(Sessions.BusinessUnitId);
            if (IsBusinessUnitEditable)
                ViewBag.IsBusinessUnitEditable = true;
            else
                ViewBag.IsBusinessUnitEditable = false;
            // End - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units

            // Added by Dharmraj Mangukiya for Deploy to integration button restrictions PL ticket #537


            //Get all subordinates of current user upto n level
            var lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            bool IsProgramEditable = false;
            if (IsBusinessUnitEditable)
            {
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
            }

            ViewBag.IsProgramEditable = IsProgramEditable;

            return PartialView("_ReviewProgram");
        }

        /// <summary>
        /// Added By: Kunal.
        /// Action to Load Campaign Setup Tab.
        /// </summary>
        /// <param name="id">Plan Campaign Id.</param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadSetupCampaign(int id)
        {

            InspectModel im = GetInspectModel(id, "campaign", false);       //// Modified by :- Sohel Pathan on 27/05/2014 for PL ticket #425
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
            }
            im.Owner = (userName.FirstName + " " + userName.LastName).ToString();

            if (im.LastSyncDate != null)
            {
                TimeZone localZone = TimeZone.CurrentTimeZone;

                ViewBag.LastSync = "Last synced with integration " + Common.GetFormatedDate(im.LastSyncDate) + ".";////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            }
            else
            {
                ViewBag.LastSync = string.Empty;
            }

            ViewBag.CampaignDetail = im;
            ViewBag.BudinessUnitTitle = db.BusinessUnits.Where(b => b.BusinessUnitId == im.BusinessUnitId && b.IsDeleted == false).Select(b => b.Title).SingleOrDefault();//Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
            ViewBag.Audience = db.Audiences.Where(a => a.AudienceId == im.AudienceId).Select(a => a.Title).SingleOrDefault();

            return PartialView("_SetupCampaign", im);
        }

        /// <summary>
        /// Added By: Kunal.
        /// Action to Load Campaign Review Tab.
        /// </summary>
        /// <param name="id">Plan Campaign Id.</param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadReviewCampaign(int id)
        {
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.Campaign).ToLower(), false);       //// Modified by :- Sohel Pathan on 27/05/2014 for PL ticket #425
            var tacticComment = (from tc in db.Plan_Campaign_Program_Tactic_Comment
                                 where tc.PlanCampaignId == id && tc.PlanCampaignId.HasValue
                                 select tc).ToArray();
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
            }

            ViewBag.ReviewModel = (from tc in tacticComment
                                   where (tc.PlanCampaignId.HasValue)
                                   select new InspectReviewModel
                                   {
                                       PlanCampaignId = Convert.ToInt32(tc.PlanCampaignId),
                                       Comment = tc.Comment,
                                       CommentDate = tc.CreatedDate,
                                       CommentedBy = userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.FirstName).FirstOrDefault() + " " + userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.LastName).FirstOrDefault(),
                                       CreatedBy = tc.CreatedBy
                                   }).ToList();

            var ownername = (from u in userName
                             where u.UserId == im.OwnerId
                             select u.FirstName + " " + u.LastName).FirstOrDefault();
            if (ownername != null)
            {
                im.Owner = ownername.ToString();
            }
            // Added BY Bhavesh
            // Calculate MQL at runtime #376
            List<Plan_Campaign_Program_Tactic> PlanTacticIds = db.Plan_Campaign_Program_Tactic.Where(ppt => ppt.Plan_Campaign_Program.PlanCampaignId == id && ppt.IsDeleted == false).ToList();
            im.MQLs = Common.GetMQLValueTacticList(PlanTacticIds).Sum(t => t.MQL);
            ViewBag.CampaignDetail = im;

            var businessunittitle = (from bun in db.BusinessUnits
                                     where bun.BusinessUnitId == im.BusinessUnitId
                                     select bun.Title).FirstOrDefault();
            ViewBag.BudinessUnitTitle = businessunittitle.ToString();

            //bool isValidDirectorUser = false;
            bool isValidOwner = false;
            //if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            //{
            //    isValidDirectorUser = true;
            //}
            if (im.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
            }
            //ViewBag.IsValidDirectorUser = isValidDirectorUser;
            ViewBag.IsValidOwner = isValidOwner;

            ViewBag.IsModelDeploy = im.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690

            //To get permission status for Approve campaign , By dharmraj PL #538
            var lstSubOrdinatesPeers = Common.GetSubOrdinatesWithPeersNLevel();
            bool isValidManagerUser = false;
            if (lstSubOrdinatesPeers.Contains(im.OwnerId) && Common.IsSectionApprovable(lstSubOrdinatesPeers, id, Enums.Section.Campaign.ToString()))////Modified by Sohel Pathan for PL ticket #688 and #689
            {
                isValidManagerUser = true;
            }
            ViewBag.IsValidManagerUser = isValidManagerUser;

            // Start - Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic
            ViewBag.IsCommentsViewEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.CommentsViewEdit);
            if ((bool)ViewBag.IsCommentsViewEditAuthorized == false)
                ViewBag.UnauthorizedCommentSection = Common.objCached.UnauthorizedCommentSection;
            // End - Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic

            // Start - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            bool IsBusinessUnitEditable = Common.IsBusinessUnitEditable(Sessions.BusinessUnitId);
            if (IsBusinessUnitEditable)
                ViewBag.IsBusinessUnitEditable = true;
            else
                ViewBag.IsBusinessUnitEditable = false;
            // End - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units

            // Added by Dharmraj Mangukiya for Deploy to integration button restrictions PL ticket #537
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
            bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            //Get all subordinates of current user upto n level
            var lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            bool IsCampaignEditable = false;
            if (IsBusinessUnitEditable)
            {
                if (im.OwnerId.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
                {
                    IsCampaignEditable = true;
                }
                else if (IsPlanEditAllAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsCampaignEditable = true;
                }
                else if (IsPlanEditSubordinatesAuthorized)
                {
                    if (lstSubOrdinates.Contains(im.OwnerId)) // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                    {
                        IsCampaignEditable = true;
                    }
                }
            }

            ViewBag.IsCampaignEditable = IsCampaignEditable;

            return PartialView("_ReviewCampaign");
        }

        #endregion

        #region "Home-Zero"
        public ActionResult Homezero()
        {
            string strURL = "#";
            MVCUrl defaultURL = Common.DefaultRedirectURL(Enums.ActiveMenu.Home);
            if (defaultURL != null)
            {
                strURL = "~/" + defaultURL.controllerName + "/" + defaultURL.actionName + defaultURL.queryString;
            }
            ViewBag.defaultURL = strURL;
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
                planmodel.objIndividuals = individuals.OrderBy(i => string.Format("{0} {1}", i.FirstName, i.LastName)).ToList();
                //end by uday

                List<TacticType> objTacticType = new List<TacticType>();

                //// Modified By: Maninder Singh for TFS Bug#282: Extra Tactics Displaying in Add Actual Screen
                objTacticType = (from t in db.Plan_Campaign_Program_Tactic
                                 where t.Plan_Campaign_Program.Plan_Campaign.PlanId == Sessions.PlanId
                                 && tacticStatus.Contains(t.Status) && t.IsDeleted == false
                                 select t.TacticType).Distinct().ToList();

                ViewBag.TacticTypeList = objTacticType;

                // Added by Dharmraj Mangukiya to implement custom restrictions PL ticket #537
                // Get current user permission for edit own and subordinates plans.
                var lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
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
            var lstUserCustomRestriction = Common.GetUserCustomRestriction();
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
            List<User> userName = objBDSUserRepository.GetMultipleTeamMemberDetails(userList, Sessions.ApplicationId);

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

        #endregion

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
                            ////

                            Plan plan = new Plan();
                            if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                            {
                                plan = db.Plan_Campaign_Program_Tactic.Single(pt => pt.PlanTacticId.Equals(planTacticId)).Plan_Campaign_Program.Plan_Campaign.Plan;
                                notificationShare = Enums.Custom_Notification.ShareTactic.ToString();
                                notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(notificationShare));
                                emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage).Replace("[URL]", Url.Action("Index", "Home", new { currentPlanId = plan.PlanId, planTacticId = planTacticId }, Request.Url.Scheme));

                            }
                            else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                            {
                                plan = db.Plan_Campaign_Program.Single(pt => pt.PlanProgramId.Equals(planTacticId)).Plan_Campaign.Plan;
                                notificationShare = Enums.Custom_Notification.ShareProgram.ToString();
                                notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(notificationShare));
                                emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage).Replace("[URL]", Url.Action("Index", "Home", new { currentPlanId = plan.PlanId, planProgramId = planTacticId }, Request.Url.Scheme));
                            }
                            else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                            {
                                plan = db.Plan_Campaign.Single(pt => pt.PlanCampaignId.Equals(planTacticId)).Plan;
                                notificationShare = Enums.Custom_Notification.ShareCampaign.ToString();
                                notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(notificationShare));
                                emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage).Replace("[URL]", Url.Action("Index", "Home", new { currentPlanId = plan.PlanId, planCampaignId = planTacticId }, Request.Url.Scheme));
                            }
                            else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                            {
                                plan = db.Plan_Improvement_Campaign_Program_Tactic.Single(pt => pt.ImprovementPlanTacticId.Equals(planTacticId)).Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan;
                                notificationShare = Enums.Custom_Notification.ShareImprovementTactic.ToString();
                                notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(notificationShare));
                                emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage).Replace("[URL]", Url.Action("Index", "Home", new { currentPlanId = plan.PlanId, planTacticId = planTacticId, isImprovement = true }, Request.Url.Scheme));
                            }

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
                Plan_Campaign_Program_Tactic planTactic = db.Plan_Campaign_Program_Tactic.Single(p => p.PlanTacticId.Equals(planTacticId));
                ViewBag.PlanTacticId = planTacticId;
                ViewBag.TacticTitle = planTactic.Title;

                //// Modified By Maninder Singh Wadhva PL Ticket#47
                ViewBag.IsImprovement = false;
            }

            else if (section == Convert.ToString(Enums.Section.Program).ToLower())
            {
                Plan_Campaign_Program planProgram = db.Plan_Campaign_Program.Single(p => p.PlanProgramId.Equals(planTacticId));
                ViewBag.PlanProgramId = planTacticId;
                ViewBag.ProgramTitle = planProgram.Title;
            }

            else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
            {
                Plan_Campaign planCampaign = db.Plan_Campaign.Single(p => p.PlanCampaignId.Equals(planTacticId));
                ViewBag.PlanCampaignId = planTacticId;
                ViewBag.CampaignTitle = planCampaign.Title;
            }
            else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
            {
                Plan_Improvement_Campaign_Program_Tactic improvementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Single(p => p.ImprovementPlanTacticId.Equals(planTacticId));
                ViewBag.PlanTacticId = planTacticId;
                ViewBag.TacticTitle = improvementTactic.Title;

                //// Modified By Maninder Singh Wadhva PL Ticket#47
                ViewBag.IsImprovement = true;
            }

            BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();
            var individuals = bdsUserRepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, true);
            if (individuals.Count != 0)
            {
                ViewBag.EmailIds = individuals.Select(member => member.Email).ToList<string>();
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
            }
            return PartialView("_ChangeLog", lst_changelog);
        }

        #endregion

        #region "Boost Method"

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Setup Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadImprovementSetup(int id)
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
            }
            im.Owner = (userName.FirstName + " " + userName.LastName).ToString();
            im.StartDate = im.StartDate;
            ViewBag.TacticDetail = im;

            var businessunittitle = (from bun in db.BusinessUnits
                                     where bun.BusinessUnitId == im.BusinessUnitId
                                     select bun.Title).FirstOrDefault();
            ViewBag.BudinessUnitTitle = businessunittitle.ToString();
            ViewBag.ApprovedStatus = true;
            ViewBag.NoOfTacticBoosts = db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted == false && t.StartDate >= im.StartDate && t.Plan_Campaign_Program.Plan_Campaign.PlanId == Sessions.PlanId).ToList().Count();


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

            return PartialView("_SetupImprovementTactic", im);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Review Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadImprovementReview(int id)
        {
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.ImprovementTactic).ToLower());
            var tacticComment = (from tc in db.Plan_Improvement_Campaign_Program_Tactic_Comment
                                 where tc.ImprovementPlanTacticId == id
                                 select tc).ToArray();
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
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

            var businessunittitle = (from bun in db.BusinessUnits
                                     where bun.BusinessUnitId == im.BusinessUnitId
                                     select bun.Title).FirstOrDefault();
            ViewBag.BudinessUnitTitle = businessunittitle.ToString();
            //bool isValidDirectorUser = false;
            bool isValidOwner = false;
            //if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            //{
            //    isValidDirectorUser = true;
            //}
            if (im.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
            }
            //ViewBag.IsValidDirectorUser = isValidDirectorUser;
            ViewBag.IsValidOwner = isValidOwner;

            //To get permission status for Approve campaign , By dharmraj PL #538
            var lstSubOrdinatesPeers = Common.GetSubOrdinatesWithPeersNLevel();
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

            // Start - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            bool IsBusinessUnitEditable = Common.IsBusinessUnitEditable(Sessions.BusinessUnitId);
            if (IsBusinessUnitEditable)
                ViewBag.IsBusinessUnitEditable = true;
            else
                ViewBag.IsBusinessUnitEditable = false;
            // End - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units

            // Added by Dharmraj Mangukiya for Deploy to integration button restrictions PL ticket #537
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
            bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            //Get all subordinates of current user upto n level
            var lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            bool IsDeployToIntegrationVisible = false;
            if (IsBusinessUnitEditable)
            {
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
            }

            ViewBag.IsDeployToIntegrationVisible = IsDeployToIntegrationVisible;

            return PartialView("_ReviewImprovementTactic");
        }

        public ActionResult LoadImprovementImpact(int id)
        {
            return PartialView("_ImpactImprovementTactic");
        }
        /// <summary>
        /// Calculate Improvenet For Tactic Type & Date.
        /// Added by Bhavesh Dobariya.
        /// </summary>
        /// <returns>JsonResult.</returns>
        public JsonResult LoadImpactImprovementStages(int ImprovementPlanTacticId)
        {
            int ImprovementTacticTypeId = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.ImprovementPlanTacticId == ImprovementPlanTacticId).Select(t => t.ImprovementTacticTypeId).SingleOrDefault();
            DateTime EffectiveDate = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.ImprovementPlanTacticId == ImprovementPlanTacticId).Select(t => t.EffectiveDate).SingleOrDefault();
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

                string strContatedIndividualList = string.Empty;
                foreach (var item in TacticUserList.Select(a => a.CreatedBy).ToList())
                {
                    strContatedIndividualList += item.ToString() + ',';
                }
                var individuals = bdsUserRepository.GetMultipleTeamMemberDetails(strContatedIndividualList.TrimEnd(','), Sessions.ApplicationId);

                return individuals;
            }
            else
            {
                //// Modified by :- Sohel Pathan on 17/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
                var TacticUserList = tacticList.Where(pcpt => status.Contains(pcpt.Status)).Select(a => a).Distinct().ToList();
                ////

                //// Custom Restrictions applied
                TacticUserList = TacticUserList.Where(t => lstAllowedVertical.Contains(t.VerticalId.ToString()) && lstAllowedGeography.Contains(t.GeographyId.ToString().ToLower())).ToList();

                string strContatedIndividualList = string.Empty;
                foreach (var item in TacticUserList.Select(a => a.CreatedBy).ToList())
                {
                    strContatedIndividualList += item.ToString() + ',';
                }
                var individuals = bdsUserRepository.GetMultipleTeamMemberDetails(strContatedIndividualList.TrimEnd(','), Sessions.ApplicationId);

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
    }
}

#region Commented
/// <summary>
/// Get Plan Campaign Program Tactic details by PlanProgramId
/// </summary>
/// <param name="planid"></param>
//public JsonResult GetPlanCampaignProgramTacticByPlanProgramId(int planprogramid, string strcat, int pid)
//{
//    try
//    {
//        //var objPlan_Campaign_Program_Tactic = db.Plan_Campaign_Program_Tactic.Where(p => p.PlanProgramId == planprogramid).Select(s => s.TacticTypeId).ToList();

//        if (strcat == "C")
//        {
//            var objPlan_Campaign_Program_Tactic = from o in db.Plan_Campaign_Program_Tactic.Where(p => p.PlanProgramId == planprogramid)
//                                                  group o by new { o.TacticTypeId } into g
//                                                  select new { id = g.Key, cnt = g.Count() };

//            if (objPlan_Campaign_Program_Tactic != null)
//            {
//                JsonConvert.SerializeObject(objPlan_Campaign_Program_Tactic, Formatting.Indented,
//                      new JsonSerializerSettings
//                      {
//                          ReferenceLoopHandling = ReferenceLoopHandling.Ignore
//                      });

//                return Json(new
//                {
//                    lstPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic,


//                }, JsonRequestBehavior.AllowGet);
//            }
//            else
//            {
//                return Json(new { }, JsonRequestBehavior.AllowGet);
//            }
//        }
//        else if (strcat == "S")
//        {
//            var objPlan = db.Plans.Where(p => p.PlanId == pid && p.IsDeleted == false).FirstOrDefault();
//            if (objPlan != null)
//            {
//                // var objModel_TacticType = db.Model_TacticType.Where(m => m.ModelId == objPlan.ModelId).OrderBy(o => o.TacticTypeId).ToList();

//                var objPlan_Campaign_Program_Tactic = from o in db.Plan_Campaign_Program_Tactic.Where(p => p.PlanProgramId == planprogramid)
//                                                      join p in db.Model_TacticType.Where(m => m.ModelId == objPlan.ModelId) on o.TacticTypeId equals p.TacticTypeId
//                                                      group o by new { p.StageId } into g
//                                                      select new { id = g.Key, cnt = g.Count() };
//                if (objPlan_Campaign_Program_Tactic != null)
//                {
//                    JsonConvert.SerializeObject(objPlan_Campaign_Program_Tactic, Formatting.Indented,
//                          new JsonSerializerSettings
//                          {
//                              ReferenceLoopHandling = ReferenceLoopHandling.Ignore
//                          });

//                    return Json(new
//                    {
//                        lstPlan_Campaign_Program_Tactic_Stage = objPlan_Campaign_Program_Tactic,


//                    }, JsonRequestBehavior.AllowGet);
//                }

//                else
//                {
//                    return Json(new { }, JsonRequestBehavior.AllowGet);
//                }

//            }


//        }
//        else
//        {
//            return Json(new { }, JsonRequestBehavior.AllowGet);
//        }
//    }
//    catch (Exception e)
//    {
//        ErrorSignal.FromCurrentContext().Raise(e);
//    }
//    return Json(new { }, JsonRequestBehavior.AllowGet);

//}
#endregion

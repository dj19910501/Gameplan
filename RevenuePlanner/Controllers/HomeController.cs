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
            //if (Sessions.RolePermission != null)
            //{
            //    Common.Permission permission = Common.GetPermission(ActionItem.Home);
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

            HomePlanModel planmodel = new Models.HomePlanModel();

            //    planmodel.objHomePlan.IsDirector = Sessions.IsDirector;

            List<Guid> businessUnitIds = new List<Guid>();

            // Modified by Dharmraj, For #537
            //if (Sessions.IsSystemAdmin)
            //{
            //    var clientBusinessUnit = db.BusinessUnits.Where(b => b.IsDeleted == false).Select(b => b.BusinessUnitId).ToList<Guid>();
            //    businessUnitIds = clientBusinessUnit.ToList();
            //    planmodel.BusinessUnitIds = Common.GetBussinessUnitIds(Sessions.User.ClientId); //commented due to not used any where
            //    ViewBag.BusinessUnitIds = planmodel.BusinessUnitIds;//Added by Nirav for Custom Dropdown - 388
            //    ViewBag.showBid = true;
            //}
            var lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList();
            if (lstAllowedBusinessUnits.Count > 0)
                lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));
            if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin) && lstAllowedBusinessUnits.Count == 0) //else if (Sessions.IsDirector || Sessions.IsClientAdmin)
            {
                var clientBusinessUnit = db.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId) && b.IsDeleted == false).Select(b => b.BusinessUnitId).ToList<Guid>();
                businessUnitIds = clientBusinessUnit.ToList();
                planmodel.BusinessUnitIds = Common.GetBussinessUnitIds(Sessions.User.ClientId); //commented due to not used any where
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
                    planmodel.BusinessUnitIds = lstClientBusinessUnits;
                    ViewBag.BusinessUnitIds = lstClientBusinessUnits.Where(a => businessUnitIds.Contains(Guid.Parse(a.Value)));
                    if (lstAllowedBusinessUnits.Count > 1)
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
                        }
                    }
                    /*added by Nirav for plan consistency on 14 apr 2014*/
                    Sessions.BusinessUnitId = currentPlan.Model.BusinessUnitId;
                    planmodel.PlanTitle = currentPlan.Title;
                    planmodel.PlanId = currentPlan.PlanId;
                    planmodel.objplanhomemodelheader = Common.GetPlanHeaderValue(currentPlan.PlanId);

                    //List<SelectListItem> planList = Common.GetPlan().Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).Take(5).ToList();

                    List<SelectListItem> UpcomingActivityList = Common.GetUpcomingActivity().Select(p => new SelectListItem() { Text = p.Text, Value = p.Value.ToString(), Selected = p.Selected }).ToList();
                    planmodel.objplanhomemodelheader.UpcomingActivity = UpcomingActivityList;

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
                TempData["ErrorMessage"] = "No published plan available to shown on home, please publish a plan.";  //// Error Message modified by Sohel Pathan on 22/05/2014 to address internal review points
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

            return PartialView("_PlanDropdown", objHomePlan);
        }
        
        #endregion

        #region "Getting list of collaborator for current plan"

        ///// <summary>
        ///// Getting list of collaborator for current plan.
        ///// </summary>
        ///// <param name="plan">Plan</param>
        ///// <returns>Returns list of collaborators for current plan.</returns>
        //private List<string> GetCollaborator(Plan plan)
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
        public JsonResult GetViewControlDetail(int type, int planId, string isQuarter, string geographyIds, string individualsIds, string activeMenu, bool isShowOnlyMyTactic = false)
        {
            #region "For all users"
            //// Setting plan id in session.
            Sessions.PlanId = planId;

            string planYear = db.Plans.Single(p => p.PlanId.Equals(planId)).Year;
            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;

            Common.GetPlanGanttStartEndDate(planYear, isQuarter, ref CalendarStartDate, ref CalendarEndDate);

            //Start Maninder Singh Wadhva : 11/15/2013 - Getting list of tactic for view control for plan version id.
            //// Selecting campaign(s) of plan whose IsDelete=false.
            var campaign = db.Plan_Campaign.Where(pc => pc.PlanId.Equals(planId) && pc.IsDeleted.Equals(false))
                                            .Select(pc => pc).ToList().Where(pc => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
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

            //// Geography Filter Criteria.
            List<Guid> filterGeography = string.IsNullOrWhiteSpace(geographyIds) ? new List<Guid>() : geographyIds.Split(',').Select(geographyId => Guid.Parse(geographyId)).ToList();

            //// Individual filter criteria.
            List<Guid> filterIndividual = string.IsNullOrWhiteSpace(individualsIds) ? new List<Guid>() : individualsIds.Split(',').Select(individualId => Guid.Parse(individualId)).ToList();

            if (isShowOnlyMyTactic)
            {
                filterIndividual.Add(Sessions.User.UserId);
            }

            //// Start Commented By :- Sohel Pathan on 16/04/2014 for PL #248 to Filter By Individual 
            //var objPlan_Campaign_Program_Tactic_Comment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.PlanTacticId != null).ToList()
            //                                            .Where(pcptc => pcptc.Plan_Campaign_Program_Tactic.IsDeleted.Equals(false) &&
            //                                                programId.Contains(pcptc.Plan_Campaign_Program_Tactic.PlanProgramId) &&
            //                                                //// Individual & Show my Tactic Filter 
            //                                               (filterIndividual.Count.Equals(0) || filterIndividual.Contains(pcptc.CreatedBy.ToString()))).ToList();

            ////// Applying filters to tactic comment (IsDelete, Individuals and Show My Tactic)
            //List<int?> tacticId = objPlan_Campaign_Program_Tactic_Comment
            //                                            .Select(pcptc => pcptc.PlanTacticId).ToList();
            //// End Commented By :- Sohel Pathan on 16/04/2014 for PL #248 to Filter By Individual 

            //// Applying filters to tactic (IsDelete, Geography, Individuals and Show My Tactic)
            var tactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted.Equals(false) &&
                                                                       programId.Contains(pcpt.PlanProgramId) &&
                //// Geography Filter
                                                                       (filterGeography.Count.Equals(0) || filterGeography.Contains(pcpt.GeographyId)) &&
                //// Individual & Show my Tactic Filter 
                //// Start Modified by :- Sohel Pathan on 14/04/2014 for PL ticket #428 Filter by individual.
                                                                       (filterIndividual.Count.Equals(0) || filterIndividual.Contains(pcpt.CreatedBy)))
                //// End Modified by :- Sohel Pathan on 14/04/2014 for PL ticket #428 Filter by individual.
                                                        .ToList()
                                                        .Where(pcpt =>
                                                            //// Checking start and end date
                                                        Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                    CalendarEndDate,
                                                                                                    pcpt.StartDate,
                                                                                                                    pcpt.EndDate).Equals(false));

            //// Modified By Maninder Singh Wadhva PL Ticket#47
            List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(improveTactic => improveTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(planId) &&
                                                                                      improveTactic.IsDeleted.Equals(false) &&
                                                                                      (improveTactic.EffectiveDate > CalendarEndDate).Equals(false))
                                                                               .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

            // Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
            var lstUserCustomRestriction = Common.GetUserCustomRestriction();
            int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            var lstAllowedVertical = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId).ToList();
            var lstAllowedGeography = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId).ToList();
            tactic = tactic.Where(t => lstAllowedVertical.Contains(t.VerticalId.ToString()) && lstAllowedGeography.Contains(t.GeographyId.ToString())).ToList();

            var lstSubordinatesWithPeers = Common.GetSubOrdinatesWithPeersNLevel();
            var subordinatesTactic = tactic.Where(t => lstSubordinatesWithPeers.Contains(t.CreatedBy)).ToList();
            var subordinatesImprovementTactic = improvementTactic.Where(t => lstSubordinatesWithPeers.Contains(t.CreatedBy)).ToList(); 

            string tacticStatus = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            //// Modified By Maninder Singh Wadhva PL Ticket#47, Modofied by Dharmraj #538
            //string requestCount = Convert.ToString(tactic.Where(t => t.Status.Equals(tacticStatus)).Count() + improvementTactic.Where(improveTactic => improveTactic.Status.Equals(tacticStatus)).Count());
            string requestCount = Convert.ToString(subordinatesTactic.Where(t => t.Status.Equals(tacticStatus)).Count() + subordinatesImprovementTactic.Where(improveTactic => improveTactic.Status.Equals(tacticStatus)).Count());

            GanttTabs currentGanttTab = (GanttTabs)type;
            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, activeMenu.ToLower());

            // Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
            if (currentGanttTab.Equals(GanttTabs.Request))
            {
                tactic = tactic.Where(t => lstSubordinatesWithPeers.Contains(t.CreatedBy)).ToList();
                improvementTactic = improvementTactic.Where(t => lstSubordinatesWithPeers.Contains(t.CreatedBy)).ToList(); 
            }

            // Added by Dharmraj Ticket #364,#365,#366
            if (objactivemenu.Equals(Enums.ActiveMenu.Plan))
            {
                List<string> status = GetStatusAsPerTab(currentGanttTab, objactivemenu);
                List<string> statusCD = new List<string>();
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

                tactic = tactic.Where(t => status.Contains(t.Status) || ((t.CreatedBy == Sessions.User.UserId && !currentGanttTab.Equals(GanttTabs.Request)) ? statusCD.Contains(t.Status) : false))
                                .Select(planTactic => planTactic)
                                .ToList<Plan_Campaign_Program_Tactic>();


                //// Modified By Maninder Singh Wadhva PL Ticket#47
                //// Show submitted/apporved/in-progress/complete improvement tactic.
                //if (objactivemenu.Equals(Enums.ActiveMenu.Home))
                //{
                List<string> improvementTacticStatus = GetStatusAsPerTab(currentGanttTab, objactivemenu);
                improvementTactic = improvementTactic.Where(improveTactic => improvementTacticStatus.Contains(improveTactic.Status) || ((improveTactic.CreatedBy == Sessions.User.UserId && !currentGanttTab.Equals(GanttTabs.Request)) ? statusCD.Contains(improveTactic.Status) : false))
                                                           .Select(improveTactic => improveTactic)
                                                           .ToList<Plan_Improvement_Campaign_Program_Tactic>();
                //}
            }
            else
            {
                List<string> status = GetStatusAsPerTab(currentGanttTab, objactivemenu);
                tactic = tactic.Where(t => status.Contains(t.Status))
                                .Select(planTactic => planTactic)
                                .ToList<Plan_Campaign_Program_Tactic>();


                //// Modified By Maninder Singh Wadhva PL Ticket#47
                //// Show submitted/apporved/in-progress/complete improvement tactic.
                //if (objactivemenu.Equals(Enums.ActiveMenu.Home))
                //{
                List<string> improvementTacticStatus = GetStatusAsPerTab(currentGanttTab, objactivemenu);
                improvementTactic = improvementTactic.Where(improveTactic => improvementTacticStatus.Contains(improveTactic.Status))
                                                           .Select(improveTactic => improveTactic)
                                                           .ToList<Plan_Improvement_Campaign_Program_Tactic>();
                //}
            }


            var improvementTacticForAccordion = GetImprovementTacticForAccordion(improvementTactic);
            var improvementTacticTypeForAccordion = GetImprovementTacticTypeForAccordion(improvementTactic);

            switch (currentGanttTab)
            {
                case GanttTabs.Tactic:
                case GanttTabs.Request:
                    #region "Tactic & Request"
                    var planCampaignProgramTactic = tactic.ToList().Select(pcpt => new
                    {
                        PlanTacticId = pcpt.PlanTacticId,
                        TacticTypeId = pcpt.TacticTypeId,
                        Title = pcpt.Title,
                        TaskId = string.Format("C{0}_P{1}_T{2}_Y{3}", pcpt.Plan_Campaign_Program.PlanCampaignId, pcpt.PlanProgramId, pcpt.PlanTacticId, pcpt.TacticTypeId),
                        //Status = pcpt.Status    //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
                    });

                    var tacticType = (tactic.Select(pcpt => pcpt.TacticType)).Select(pcpt => new
                    {
                        pcpt.TacticTypeId,
                        pcpt.Title,
                        ColorCode = pcpt.ColorCode
                    }).Distinct().OrderBy(pcpt => pcpt.Title);

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    List<object> tacticAndRequestTaskData = GetTaskDetailTactic(tactic.ToList(), improvementTactic);
                    tacticAndRequestTaskData = Common.AppendImprovementTaskData(tacticAndRequestTaskData, improvementTactic, CalendarStartDate, CalendarEndDate, false);

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    return Json(new
                    {
                        planCampaignProgramTactic = planCampaignProgramTactic.ToList(),
                        tacticType = tacticType.ToList(),
                        taskData = tacticAndRequestTaskData,
                        requestCount = requestCount,
                        planYear = planYear,
                        improvementTacticForAccordion = improvementTacticForAccordion,
                        improvementTacticTypeForAccordion = improvementTacticTypeForAccordion
                    }, JsonRequestBehavior.AllowGet);
                    #endregion
                case GanttTabs.Vertical:
                    #region Vertical
                    var verticalTactic = tactic.ToList().Select(pcpt => new
                            {
                                PlanTacticId = pcpt.PlanTacticId,
                                VerticalId = pcpt.VerticalId,
                                Title = pcpt.Title,
                                TaskId = string.Format("V{0}_C{1}_P{2}_T{3}", pcpt.VerticalId, pcpt.Plan_Campaign_Program.PlanCampaignId, pcpt.PlanProgramId, pcpt.PlanTacticId),
                                //Status = pcpt.Status    //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
                            });

                    var verticals = (tactic.Select(vertical => vertical.Vertical)).Select(vertical => new
                    {
                        VerticalId = vertical.VerticalId,
                        Title = vertical.Title,
                        ColorCode = vertical.ColorCode
                    }).Distinct().OrderBy(vertical => vertical.Title);

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    List<object> verticalTaskData = GetTaskDetailVertical(campaign.ToList(), program.ToList(), tactic.ToList(), improvementTactic);
                    verticalTaskData = Common.AppendImprovementTaskData(verticalTaskData, improvementTactic, CalendarStartDate, CalendarEndDate, false);

                    return Json(new
                    {
                        verticalTactic = verticalTactic.ToList(),
                        vertical = verticals.ToList(),
                        taskData = verticalTaskData,
                        requestCount = requestCount,
                        planYear = planYear,
                        improvementTacticForAccordion = improvementTacticForAccordion,
                        improvementTacticTypeForAccordion = improvementTacticTypeForAccordion
                    }, JsonRequestBehavior.AllowGet);
                    #endregion
                case GanttTabs.Stage:
                    #region Stage
                    //// Modified by Maninder Singh Wadhva on 06/13/2014 for PL #526 Home & Plan Pages: Stages are different but all Tactics are displayed under SUS
                    var queryStages = tactic.Select(t => new
                    {
                        t.StageId,
                        t.Stage.Title,
                        t.Stage.ColorCode,
                        //Status = t.Status   //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
                    }).Distinct();

                    var queryStageTacticType = tactic.Select(t => new
                    {
                        t.PlanTacticId,
                        t.Title,
                        t.StageId,
                        TaskId = string.Format("S{0}_C{1}_P{2}_T{3}", t.StageId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId, t.PlanTacticId),
                        //Status = t.Status   //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
                    });

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    List<object> stageTaskData = GetTaskDetailStage(campaign.ToList(), program.ToList(), tactic.ToList(), improvementTactic);
                    stageTaskData = Common.AppendImprovementTaskData(stageTaskData, improvementTactic, CalendarStartDate, CalendarEndDate, false);

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    return Json(new
                    {
                        //// For View Control.
                        stageTactic = queryStageTacticType.ToList(),

                        //// For View Control.
                        stage = queryStages.ToList(),

                        //// For Gantt.
                        taskData = stageTaskData,
                        requestCount = requestCount,
                        planYear = planYear,
                        improvementTacticForAccordion = improvementTacticForAccordion,
                        improvementTacticTypeForAccordion = improvementTacticTypeForAccordion
                    }, JsonRequestBehavior.AllowGet);
                    #endregion
                case GanttTabs.Audience:
                    #region Audience
                    var audienceTactic = tactic.ToList().Select(pcpt => new
                           {
                               PlanTacticId = pcpt.PlanTacticId,
                               AudienceId = pcpt.AudienceId,
                               Title = pcpt.Title,
                               TaskId = string.Format("A{0}_C{1}_P{2}_T{3}", pcpt.AudienceId, pcpt.Plan_Campaign_Program.PlanCampaignId, pcpt.PlanProgramId, pcpt.PlanTacticId),
                               //Status = pcpt.Status     //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
                           });

                    var audiences = (tactic.Select(audience => audience.Audience)).Select(audience => new
                    {
                        audience.AudienceId,
                        audience.Title,
                        ColorCode = audience.ColorCode
                    }).Distinct().OrderBy(audience => audience.Title);

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    List<object> audienceTaskData = GetTaskDetailAudience(campaign.ToList(), program.ToList(), tactic.ToList(), improvementTactic);
                    audienceTaskData = Common.AppendImprovementTaskData(audienceTaskData, improvementTactic, CalendarStartDate, CalendarEndDate, false);

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    return Json(new
                    {
                        audienceTactic = audienceTactic.ToList(),
                        audience = audiences.ToList(),
                        taskData = audienceTaskData,
                        requestCount = requestCount,
                        planYear = planYear,
                        improvementTacticForAccordion = improvementTacticForAccordion,
                        improvementTacticTypeForAccordion = improvementTacticTypeForAccordion
                    }, JsonRequestBehavior.AllowGet);
                    #endregion
                case GanttTabs.BusinessUnit:
                    #region "Business Unit"
                    var plan = db.Plans.Single(p => p.PlanId.Equals(planId));

                    BusinessUnit businessUnit = plan.Model.BusinessUnit;

                    //// Business and Tactic type.
                    var queryBusinessUnitTacticType = tactic.Select(pt => new
                    {
                        pt.PlanTacticId,
                        pt.Title,
                        businessUnit.BusinessUnitId,
                        TaskId = string.Format("B{0}_C{1}_P{2}_T{3}", businessUnit.BusinessUnitId, pt.Plan_Campaign_Program.PlanCampaignId, pt.PlanProgramId, pt.PlanTacticId),
                        //Status = pt.Status      //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
                    });

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    List<object> businessUnitTaskData = GetTaskDetailBusinessUnit(businessUnit, tactic.ToList(), improvementTactic);
                    businessUnitTaskData = Common.AppendImprovementTaskData(businessUnitTaskData, improvementTactic, CalendarStartDate, CalendarEndDate, false);

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    if (queryBusinessUnitTacticType.Count() > 0)
                    {
                        return Json(new
                          {
                              //// For View Control.
                              businessUnitTactic = queryBusinessUnitTacticType.ToList(),

                              //// For View Control.
                              businessUnit = new[] { new { businessUnit.BusinessUnitId, businessUnit.Title, businessUnit.ColorCode } },

                              //// For Gantt.
                              taskData = businessUnitTaskData,
                              requestCount = requestCount,
                              planYear = planYear,
                              improvementTacticForAccordion = improvementTacticForAccordion,
                              improvementTacticTypeForAccordion = improvementTacticTypeForAccordion
                          }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { businessUnitTactic = "", businessUnit = "", taskData = "", planYear = planYear }, JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                default:
                    return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            #endregion
        }

        /// <summary>
        /// //Function to get current plan edit permission, PL #538
        /// Added by Dharmraj
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        public JsonResult GetCurrentPlanPermissionDetail(int planId)
        {
            // To get permission status for Plan Edit , By dharmraj PL #519
            //Get all subordinates of current user upto n level
            bool IsPlanEditable = true;
            var lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            lstOwnAndSubOrdinates.Add(Sessions.User.UserId);
            // Get current user permission for edit own and subordinates plans.
            bool IsPlanEditOwnAndSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditOwnAndSubordinates);
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
            var objPlan = db.Plans.FirstOrDefault(p => p.PlanId == Sessions.PlanId);
            bool IsBusinessUnitEditable = Common.IsBusinessUnitEditable(Sessions.BusinessUnitId);   // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            if (IsPlanEditAllAuthorized && IsBusinessUnitEditable)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                IsPlanEditable = true;
            }
            else if (IsPlanEditOwnAndSubordinatesAuthorized)
            {
                if (lstOwnAndSubOrdinates.Contains(objPlan.CreatedBy) && IsBusinessUnitEditable)    // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsPlanEditable = true;
                }
                else
                {
                    IsPlanEditable = false;
                }
            }
            else
            {
                IsPlanEditable = false;
            }

            return Json(new { IsPlanEditable = IsPlanEditable }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By Maninder Singh Wadhva PL Ticket#47
        /// Modified By Dharmraj PL Ticket#364 & Ticket#365 & Ticket#366
        /// Function to get status as per tab.
        /// </summary>
        /// <param name="currentTab">Current Tab.</param>
        /// <returns>Returns list of status as per tab.</returns>
        private List<string> GetStatusAsPerTab(GanttTabs currentTab, Enums.ActiveMenu objactivemenu)
        {
            List<string> status = new List<string>();

            if (currentTab.Equals(GanttTabs.Request))
            {
                status.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            }
            else
            {
                status = Common.GetStatusListAfterApproved();
                if (objactivemenu.Equals(Enums.ActiveMenu.Plan))
                {
                    status.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
                    //status.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                    //status.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());
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
                TaskId = string.Format("M{0}_I{1}_Y{2}", improvementPlanCampaignId, improvementTactic.ImprovementPlanTacticId, improvementTactic.ImprovementTacticTypeId)
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
        private List<object> GetTaskDetailBusinessUnit(BusinessUnit businessUnit, List<Plan_Campaign_Program_Tactic> planTactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        {
            //// Business Unit.

            if (planTactic.Count > 0)
            {
                var queryBusinessUnitTacticType = new
                {
                    id = string.Format("B{0}", businessUnit.BusinessUnitId),
                    text = businessUnit.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateStageAndBusinessUnit(planTactic.Select(c => c.Plan_Campaign_Program.Plan_Campaign).ToList(),
                                                                            planTactic.Select(c => c.Plan_Campaign_Program).ToList(),
                                                                            planTactic.Select(c => c).ToList())),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                              CalendarEndDate,
                                GetMinStartDateStageAndBusinessUnit(planTactic.Select(c => c.Plan_Campaign_Program.Plan_Campaign).ToList(),
                                                                            planTactic.Select(c => c.Plan_Campaign_Program).ToList(),
                                                                            planTactic.Select(c => c).ToList()),
                                                              GetMaxEndDateStageAndBusinessUnit(planTactic.Select(c => c.Plan_Campaign_Program.Plan_Campaign).ToList(),
                                                                            planTactic.Select(c => c.Plan_Campaign_Program).ToList(),
                                                                            planTactic.Select(c => c).ToList())),
                    progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateStageAndBusinessUnit(planTactic.Select(c => c.Plan_Campaign_Program.Plan_Campaign).ToList(),
                                                                            planTactic.Select(c => c.Plan_Campaign_Program).ToList(),
                                                                            planTactic.Select(c => c).ToList())),
                                                       Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                              CalendarEndDate,
                                                                GetMinStartDateStageAndBusinessUnit(planTactic.Select(c => c.Plan_Campaign_Program.Plan_Campaign).ToList(),
                                                                            planTactic.Select(c => c.Plan_Campaign_Program).ToList(),
                                                                            planTactic.Select(c => c).ToList()),
                                                                GetMaxEndDateStageAndBusinessUnit(planTactic.Select(c => c.Plan_Campaign_Program.Plan_Campaign).ToList(),
                                                                            planTactic.Select(c => c.Plan_Campaign_Program).ToList(),
                                                                            planTactic.Select(c => c).ToList())), planTactic, improvementTactic),//progress = 0,
                    open = false,
                    color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, businessUnit.ColorCode.ToLower())
                };

                var newQueryBusinessUnitTacticType = new
                {
                    id = queryBusinessUnitTacticType.id,
                    text = queryBusinessUnitTacticType.text,
                    start_date = queryBusinessUnitTacticType.start_date,
                    duration = queryBusinessUnitTacticType.duration,
                    progress = queryBusinessUnitTacticType.progress,
                    open = queryBusinessUnitTacticType.open,
                    color = queryBusinessUnitTacticType.color + ((queryBusinessUnitTacticType.progress > 0) ? "stripe" : "")
                };

                //// Tactic
                var taskDataTactic = planTactic.Select(bt => new
                {
                    id = string.Format("B{0}_C{1}_P{2}_T{3}", businessUnit.BusinessUnitId, bt.Plan_Campaign_Program.PlanCampaignId, bt.Plan_Campaign_Program.PlanProgramId, bt.PlanTacticId),
                    text = bt.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, bt.StartDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                              CalendarEndDate,
                                                              bt.StartDate,
                                                              bt.EndDate),
                    progress = GetTacticProgress(bt, improvementTactic), //progress = 0,
                    open = false,
                    parent = string.Format("B{0}_C{1}_P{2}", businessUnit.BusinessUnitId, bt.Plan_Campaign_Program.PlanCampaignId, bt.Plan_Campaign_Program.PlanProgramId),
                    color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, businessUnit.ColorCode.ToLower()),
                    plantacticid = bt.PlanTacticId,
                    Status = bt.Status      //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
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
                    Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
                });

                //// Program
                var taskDataProgram = planTactic.Select(bt => new
                {
                    id = string.Format("B{0}_C{1}_P{2}", businessUnit.BusinessUnitId, bt.Plan_Campaign_Program.PlanCampaignId, bt.Plan_Campaign_Program.PlanProgramId),
                    text = bt.Plan_Campaign_Program.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, bt.Plan_Campaign_Program.StartDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                              CalendarEndDate,
                                                              bt.Plan_Campaign_Program.StartDate,
                                                              bt.Plan_Campaign_Program.EndDate),
                    progress = GetProgramProgress(planTactic, bt.Plan_Campaign_Program, improvementTactic),//progress = 0,
                    open = false,
                    parent = string.Format("B{0}_C{1}", businessUnit.BusinessUnitId, bt.Plan_Campaign_Program.PlanCampaignId),
                    color = "",
                    planprogramid = bt.Plan_Campaign_Program.PlanProgramId,
                    Status = bt.Plan_Campaign_Program.Status        //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
                }).Select(p => p).Distinct().OrderBy(p => p.text);

                var newTaskDataProgram = taskDataProgram.Select(t => new
                {
                    id = t.id,
                    text = t.text,
                    start_date = t.start_date,
                    duration = t.duration,
                    progress = t.progress,
                    open = t.open,
                    parent = t.parent,
                    color = (t.progress == 1 ? " stripe stripe-no-border " : (t.progress > 0 ? "partialStripe" : "")),
                    planprogramid = t.planprogramid,
                    Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
                });

                //// Campaign
                var taskDataCampaign = planTactic.Select(bt => new
                {
                    id = string.Format("B{0}_C{1}", businessUnit.BusinessUnitId, bt.Plan_Campaign_Program.PlanCampaignId),
                    text = bt.Plan_Campaign_Program.Plan_Campaign.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, bt.Plan_Campaign_Program.Plan_Campaign.StartDate),
                    duration = (bt.Plan_Campaign_Program.Plan_Campaign.EndDate - bt.Plan_Campaign_Program.Plan_Campaign.StartDate).TotalDays,
                    progress = GetCampaignProgress(planTactic, bt.Plan_Campaign_Program.Plan_Campaign, improvementTactic),//progress = 0,
                    open = false,
                    parent = string.Format("B{0}", businessUnit.BusinessUnitId),
                    color = Common.COLORC6EBF3_WITH_BORDER,
                    plancampaignid = bt.Plan_Campaign_Program.PlanCampaignId,
                    Status = bt.Plan_Campaign_Program.Plan_Campaign.Status      //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
                }).Select(c => c).Distinct().OrderBy(c => c.text);

                var newTaskDataCampaign = taskDataCampaign.Select(t => new
                {
                    id = t.id,
                    text = t.text,
                    start_date = t.start_date,
                    duration = t.duration,
                    progress = t.progress,
                    open = t.open,
                    parent = t.parent,
                    color = t.color + (t.progress == 1 ? " stripe" : (t.progress > 0 ? "stripe" : "")),
                    plancampaignid = t.plancampaignid,
                    Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
                });

                //return taskDataCampaign.Concat<object>(taskDataProgram).Concat<object>(taskDataTactic).Concat(new[] { queryBusinessUnitTacticType }).ToList<object>();
                return newTaskDataCampaign.Concat<object>(newTaskDataProgram).Concat<object>(newTaskDataTactic).Concat(new[] { newQueryBusinessUnitTacticType }).ToList<object>();
            }
            else
                return null;

        }

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
        public List<object> GetTaskDetailAudience(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        {
            var taskDataAudience = tactic.Select(t => new
            {
                id = string.Format("A{0}", t.AudienceId),
                text = t.Audience.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDate(GanttTabs.Audience, t.AudienceId, campaign, program, tactic)),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          GetMinStartDate(GanttTabs.Audience, t.AudienceId, campaign, program, tactic),
                                                          GetMaxEndDate(GanttTabs.Audience, t.AudienceId, campaign, program, tactic)),
                progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDate(GanttTabs.Audience, t.AudienceId, campaign, program, tactic)),
                                               Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                         CalendarEndDate,
                                                         GetMinStartDate(GanttTabs.Audience, t.AudienceId, campaign, program, tactic),
                                                         GetMaxEndDate(GanttTabs.Audience, t.AudienceId, campaign, program, tactic)), tactic, improvementTactic),//progress = 0,
                open = false,
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Audience.ColorCode.ToLower()),
                //Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            }).Select(a => a).Distinct().OrderBy(t => t.text);

            var newTaskDataAudience = taskDataAudience.Select(t => new
            {
                id = t.id,
                text = t.text,
                start_date = t.start_date,
                duration = t.duration,
                progress = t.progress,
                open = t.open,
                color = t.color + ((t.progress > 0) ? "stripe" : ""),
                //Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            });

            var taskDataCampaign = tactic.Select(t => new
            {
                id = string.Format("A{0}_C{1}", t.AudienceId, t.Plan_Campaign_Program.PlanCampaignId),
                text = t.Plan_Campaign_Program.Plan_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.Plan_Campaign.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.Plan_Campaign_Program.Plan_Campaign.StartDate,
                                                          t.Plan_Campaign_Program.Plan_Campaign.EndDate),
                progress = GetCampaignProgress(tactic, t.Plan_Campaign_Program.Plan_Campaign, improvementTactic),//progress = 0,
                open = false,
                parent = string.Format("A{0}", t.AudienceId),
                color = Common.COLORC6EBF3_WITH_BORDER,
                plancampaignid = t.Plan_Campaign_Program.PlanCampaignId,
                Status = t.Plan_Campaign_Program.Plan_Campaign.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            }).Select(t => t).Distinct().OrderBy(t => t.text);

            var newTaskDataCampaign = taskDataCampaign.Select(t => new
            {
                id = t.id,
                text = t.text,
                start_date = t.start_date,
                duration = t.duration,
                progress = t.progress,
                open = t.open,
                parent = t.parent,
                color = t.color + (t.progress == 1 ? " stripe" : (t.progress > 0 ? "stripe" : "")),
                plancampaignid = t.plancampaignid,
                Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            });

            var taskDataProgram = tactic.Select(t => new
            {
                id = string.Format("A{0}_C{1}_P{2}", t.AudienceId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),
                text = t.Plan_Campaign_Program.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.Plan_Campaign_Program.StartDate,
                                                          t.Plan_Campaign_Program.EndDate),
                progress = GetProgramProgress(tactic, t.Plan_Campaign_Program, improvementTactic),//progress = 0,
                open = false,
                parent = string.Format("A{0}_C{1}", t.AudienceId, t.Plan_Campaign_Program.PlanCampaignId),
                color = "",
                planprogramid = t.PlanProgramId,
                Status = t.Plan_Campaign_Program.Status     //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            }).Select(t => t).Distinct().OrderBy(t => t.text);


            var newTaskDataProgram = taskDataProgram.Select(t => new
            {
                id = t.id,
                text = t.text,
                start_date = t.start_date,
                duration = t.duration,
                progress = t.progress,
                open = t.open,
                parent = t.parent,
                color = (t.progress == 1 ? " stripe stripe-no-border " : (t.progress > 0 ? "partialStripe" : "")),
                planprogramid = t.planprogramid,
                Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            });

            var taskDataTactic = tactic.Select(t => new
            {
                id = string.Format("A{0}_C{1}_P{2}_T{3}", t.AudienceId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId),
                text = t.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.StartDate,
                                                          t.EndDate),
                progress = GetTacticProgress(t, improvementTactic), //progress = 0,
                open = false,
                parent = string.Format("A{0}_C{1}_P{2}", t.AudienceId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Audience.ColorCode.ToLower()),
                plantacticid = t.PlanTacticId,
                Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
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
                Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            });

            //return taskDataAudience.Concat<object>(taskDataCampaign).Concat<object>(taskDataTactic).Concat<object>(taskDataProgram).ToList<object>();
            return newTaskDataAudience.Concat<object>(newTaskDataCampaign).Concat<object>(newTaskDataTactic).Concat<object>(newTaskDataProgram).ToList<object>();
        }

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
        public List<object> GetTaskDetailStage(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        {
            //// Stage
            var taskStages = tactic.Select(t => new
            {
                id = string.Format("S{0}", t.StageId),
                text = t.Stage.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(t.StageId)).ToList<Plan_Campaign_Program_Tactic>())),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(t.StageId)).ToList<Plan_Campaign_Program_Tactic>()),
                                                          GetMaxEndDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(t.StageId)).ToList<Plan_Campaign_Program_Tactic>())),
                progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(t.StageId)).ToList<Plan_Campaign_Program_Tactic>())),
                                            Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(t.StageId)).ToList<Plan_Campaign_Program_Tactic>()),
                                                          GetMaxEndDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.StageId.Equals(t.StageId)).ToList<Plan_Campaign_Program_Tactic>())),
                                                          tactic, improvementTactic),//progress = 0,
                open = false,
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Stage.ColorCode.ToLower()),
                //Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            }).Distinct();

            var newTaskStages = taskStages.Select(s => new
            {
                id = s.id,
                text = s.text,
                start_date = s.start_date,
                duration = s.duration,
                progress = s.progress,
                open = s.open,
                color = s.color + ((s.progress > 0) ? "stripe" : ""),
                //Status = s.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            });

            //// Tactic
            var taskDataTactic = tactic.Select(t => new
            {
                id = string.Format("S{0}_C{1}_P{2}_T{3}", t.StageId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId),
                text = t.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.StartDate,
                                                          t.EndDate),
                progress = GetTacticProgress(t, improvementTactic), //progress = 0,
                open = false,
                parent = string.Format("S{0}_C{1}_P{2}", t.StageId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Stage.ColorCode.ToLower()),
                plantacticid = t.PlanTacticId,
                Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
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
                Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            });

            //// Program
            var taskDataProgram = tactic.Select(t => new
            {
                id = string.Format("S{0}_C{1}_P{2}", t.StageId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),
                text = t.Plan_Campaign_Program.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.Plan_Campaign_Program.StartDate,
                                                          t.Plan_Campaign_Program.EndDate),
                progress = GetProgramProgress(tactic, t.Plan_Campaign_Program, improvementTactic),//progress = 0,
                open = false,
                parent = string.Format("S{0}_C{1}", t.StageId, t.Plan_Campaign_Program.PlanCampaignId),
                color = "",
                planprogramid = t.PlanProgramId,
                Status = t.Plan_Campaign_Program.Status     //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
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

            //// Campaign
            var taskDataCampaign = tactic.Select(t => new
            {
                id = string.Format("S{0}_C{1}", t.StageId, t.Plan_Campaign_Program.PlanCampaignId),
                text = t.Plan_Campaign_Program.Plan_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.Plan_Campaign.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.Plan_Campaign_Program.Plan_Campaign.StartDate,
                                                          t.Plan_Campaign_Program.Plan_Campaign.EndDate),
                progress = GetCampaignProgress(tactic, t.Plan_Campaign_Program.Plan_Campaign, improvementTactic),//progress = 0,
                open = false,
                parent = string.Format("S{0}", t.StageId),
                color = Common.COLORC6EBF3_WITH_BORDER,
                plancampaignid = t.Plan_Campaign_Program.PlanCampaignId,
                Status = t.Plan_Campaign_Program.Plan_Campaign.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
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

            //return taskStages.Concat<object>(taskDataCampaign).Concat<object>(taskDataProgram).Concat<object>(taskDataTactic).ToList<object>();
            return newTaskStages.Concat<object>(newTaskDataCampaign).Concat<object>(newTaskDataProgram).Concat<object>(newTaskDataTactic).ToList<object>();
        }

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
        public List<object> GetTaskDetailVertical(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        {
            var taskDataVertical = tactic.Select(t => new
            {
                id = string.Format("V{0}", t.VerticalId),
                text = t.Vertical.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDate(GanttTabs.Vertical, t.VerticalId, campaign, program, tactic)),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          GetMinStartDate(GanttTabs.Vertical, t.VerticalId, campaign, program, tactic),
                                                          GetMaxEndDate(GanttTabs.Vertical, t.VerticalId, campaign, program, tactic)),
                progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDate(GanttTabs.Vertical, t.VerticalId, campaign, program, tactic)),
                                                Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          GetMinStartDate(GanttTabs.Vertical, t.VerticalId, campaign, program, tactic),
                                                          GetMaxEndDate(GanttTabs.Vertical, t.VerticalId, campaign, program, tactic)),
                                               tactic, improvementTactic),//progress = 0,
                open = false,
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Vertical.ColorCode.ToLower()),
                //Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            }).Select(v => v).Distinct().OrderBy(t => t.text);

            var newTaskDataVertical = taskDataVertical.Select(v => new
            {
                id = v.id,
                text = v.text,
                start_date = v.start_date,
                duration = v.duration,
                progress = v.progress,
                open = v.open,
                color = v.color + ((v.progress > 0) ? "stripe" : ""),
                //Status = v.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            });

            var taskDataCampaign = tactic.Select(t => new
            {
                id = string.Format("V{0}_C{1}", t.VerticalId, t.Plan_Campaign_Program.PlanCampaignId),
                text = t.Plan_Campaign_Program.Plan_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.Plan_Campaign.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.Plan_Campaign_Program.Plan_Campaign.StartDate,
                                                          t.Plan_Campaign_Program.Plan_Campaign.EndDate),
                progress = GetCampaignProgress(tactic, t.Plan_Campaign_Program.Plan_Campaign, improvementTactic),//progress = 0,
                open = false,
                parent = string.Format("V{0}", t.VerticalId),
                color = Common.COLORC6EBF3_WITH_BORDER,
                plancampaignid = t.Plan_Campaign_Program.PlanCampaignId,
                Status = t.Plan_Campaign_Program.Plan_Campaign.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            }).Select(t => t).Distinct().OrderBy(t => t.text);

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

            var taskDataProgram = tactic.Select(t => new
           {
               id = string.Format("V{0}_C{1}_P{2}", t.VerticalId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),
               text = t.Plan_Campaign_Program.Title,
               start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.StartDate),
               duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.Plan_Campaign_Program.StartDate,
                                                          t.Plan_Campaign_Program.EndDate),
               progress = GetProgramProgress(tactic, t.Plan_Campaign_Program, improvementTactic),//progress = 0,
               open = false,
               parent = string.Format("V{0}_C{1}", t.VerticalId, t.Plan_Campaign_Program.PlanCampaignId),
               color = "",
               planprogramid = t.PlanProgramId,
               Status = t.Plan_Campaign_Program.Status      //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
           }).Select(t => t).Distinct().OrderBy(t => t.text);

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

            var taskDataTactic = tactic.Select(t => new
            {
                id = string.Format("V{0}_C{1}_P{2}_T{3}", t.VerticalId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId),
                text = t.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.StartDate,
                                                          t.EndDate),
                progress = GetTacticProgress(t, improvementTactic), //progress = 0,
                open = false,
                parent = string.Format("V{0}_C{1}_P{2}", t.VerticalId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Vertical.ColorCode.ToLower()),
                plantacticid = t.PlanTacticId,
                Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
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
                Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            });

            //return taskDataVertical.Concat<object>(taskDataCampaign).Concat<object>(taskDataTactic).Concat<object>(taskDataProgram).ToList<object>();
            return newTaskDataVertical.Concat<object>(newTaskDataCampaign).Concat<object>(newTaskDataTactic).Concat<object>(newTaskDataProgram).ToList<object>();
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
        public List<object> GetTaskDetailTactic(List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
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
                id = string.Format("C{0}_P{1}_T{2}_Y{3}", t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId, t.TacticTypeId),
                text = t.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.StartDate,
                                                          t.EndDate),
                progress = GetTacticProgress(t, improvementTactic), //progress = 0,
                open = false,
                parent = string.Format("C{0}_P{1}", t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId),
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
                id = string.Format("C{0}_P{1}", p.Plan_Campaign_Program.PlanCampaignId, p.PlanProgramId),
                text = p.Plan_Campaign_Program.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, p.Plan_Campaign_Program.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          p.Plan_Campaign_Program.StartDate,
                                                          p.Plan_Campaign_Program.EndDate),
                progress = GetProgramProgress(tactic, p.Plan_Campaign_Program, improvementTactic),//progress = 0,
                open = false,
                parent = string.Format("C{0}", p.Plan_Campaign_Program.PlanCampaignId),
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
                id = string.Format("C{0}", c.Plan_Campaign_Program.PlanCampaignId),
                text = c.Plan_Campaign_Program.Plan_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, c.Plan_Campaign_Program.Plan_Campaign.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          c.Plan_Campaign_Program.Plan_Campaign.StartDate,
                                                          c.Plan_Campaign_Program.Plan_Campaign.EndDate),
                progress = GetCampaignProgress(tactic, c.Plan_Campaign_Program.Plan_Campaign, improvementTactic),//progress = 0,
                open = false,
                color = Common.COLORC6EBF3_WITH_BORDER,
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
                color = c.color + (c.progress == 1 ? " stripe" : (c.progress > 0 ? "stripe" : "")),
                plancampaignid = c.plancampaignid,
                Status = c.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            });

            //return taskDataCampaign.Concat<object>(taskDataTactic).Concat<object>(taskDataProgram).ToList<object>();
            return newTaskDataCampaign.Concat<object>(NewTaskDataTactic).Concat<object>(newTaskDataProgram).ToList<object>();
        }

        /// <summary>
        /// Function to get tactic progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 2nd april, 2013
        /// <param name="planCampaignProgramTactic"></param>
        /// <param name="improvementTactic"></param>
        /// <returns>return progress B/W 0 and 1</returns>
        public double GetTacticProgress(Plan_Campaign_Program_Tactic planCampaignProgramTactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        {
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
        /// <param name="planCampaignProgram"></param>
        /// <param name="improvementTactic"></param>
        /// <returns>return progress B/W 0 and 1</returns>
        public double GetProgramProgress(List<Plan_Campaign_Program_Tactic> tactic, Plan_Campaign_Program planCampaignProgram, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        {
            if (improvementTactic.Count > 0)
            {
                DateTime minDate = improvementTactic.Select(t => t.EffectiveDate).Min(); // Minimun date of improvement tactic

                // Start date of program
                DateTime programStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaignProgram.StartDate));

                // List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = tactic.Where(p => p.IsDeleted.Equals(false) && p.PlanProgramId == planCampaignProgram.PlanProgramId && (p.StartDate > minDate).Equals(true))
                                                                                              .Select(t => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate)) })
                                                                                              .ToList();

                if (lstAffectedTactic.Count > 0)
                {
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(t => t.startDate).Min(); // minimum start Date of tactics
                    if (tacticMinStartDate > minDate) // If any tactic affected by at least one improvement tactic
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
        public double GetCampaignProgress(List<Plan_Campaign_Program_Tactic> tactic, Plan_Campaign planCampaign, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        {
            if (improvementTactic.Count > 0)
            {
                DateTime minDate = improvementTactic.Select(t => t.EffectiveDate).Min(); // Minimun date of improvement tactic

                // Start date of Campaign
                DateTime campaignStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaign.StartDate));

                // List of all tactics
                var lstTactic = tactic.Where(p => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
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
        /// Function to get progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 3rd april, 2013
        /// </summary>
        /// <param name="StartDate"></param>
        /// <param name="Duration"></param>
        /// <param name="tactic"></param>
        /// <param name="improvementTactic"></param>
        /// <returns>return progress B/W 0 and 1</returns>
        public double GetProgress(string taskStartDate, double taskDuration, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        {
            if (improvementTactic.Count > 0)
            {
                DateTime minDate = improvementTactic.Select(t => t.EffectiveDate).Min(); // Minimun date of improvement tactic

                // List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = tactic.Where(p => (p.StartDate > minDate).Equals(true))
                                                 .Select(t => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate)) })
                                                 .ToList();

                if (lstAffectedTactic.Count > 0)
                {
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(t => t.startDate).Min(); // minimum start Date of tactics
                    if (tacticMinStartDate > minDate) // If any tactic affected by at least one improvement tactic.
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
        public DateTime GetMinStartDate(GanttTabs currentGanttTab, int typeId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
        {
            var queryPlanProgramId = new List<int>();

            DateTime minDateTactic = DateTime.Now;
            switch (currentGanttTab)
            {
                case GanttTabs.Vertical:
                    queryPlanProgramId = tactic.Where(t => t.VerticalId == typeId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.VerticalId == typeId).Select(t => t.StartDate).Min();
                    break;
                case GanttTabs.Audience:
                    queryPlanProgramId = tactic.Where(t => t.AudienceId == typeId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.AudienceId == typeId).Select(t => t.StartDate).Min();
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
        public DateTime GetMaxEndDate(GanttTabs currentGanttTab, int typeId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
        {
            var queryPlanProgramId = new List<int>();

            DateTime maxDateTactic = DateTime.Now;

            switch (currentGanttTab)
            {
                case GanttTabs.Vertical:
                    queryPlanProgramId = tactic.Where(t => t.VerticalId == typeId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.VerticalId == typeId).Select(t => t.EndDate).Max();
                    break;
                case GanttTabs.Audience:
                    queryPlanProgramId = tactic.Where(t => t.AudienceId == typeId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.AudienceId == typeId).Select(t => t.EndDate).Max();
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
                                tactic.Status = status;
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
                                    if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
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
                lstOwnAndSubOrdinates.Add(Sessions.User.UserId);
                // Get current user permission for edit own and subordinates plans.
                bool IsPlanEditOwnAndSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditOwnAndSubordinates);
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                var objPlan = db.Plans.FirstOrDefault(p => p.PlanId == Sessions.PlanId);
                bool IsPlanEditable = false;
                bool IsBusinessUnitEditable = Common.IsBusinessUnitEditable(Sessions.BusinessUnitId);   // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                if (IsPlanEditAllAuthorized && IsBusinessUnitEditable)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsPlanEditable = true;//ViewBag.IsPlanEditable = true;
                }
                else if (IsPlanEditOwnAndSubordinatesAuthorized)
                {
                    if (lstOwnAndSubOrdinates.Contains(objPlan.CreatedBy) && IsBusinessUnitEditable)    // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                    {
                        IsPlanEditable = true;
                    }
                    else
                    {
                        IsPlanEditable = false;
                    }
                }
                else
                {
                    IsPlanEditable = false;
                }

                if (Convert.ToString(section) != "")
                {
                    string verticalId = string.Empty;
                    string GeographyId = string.Empty;
                    if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        Plan_Campaign_Program_Tactic objPlan_Campaign_Program_Tactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(id)).SingleOrDefault();
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
                        Plan_Campaign_Program objPlan_Campaign_Program = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(id)).SingleOrDefault();
                        verticalId = objPlan_Campaign_Program.VerticalId == null ? string.Empty : objPlan_Campaign_Program.VerticalId.ToString();
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
                        Plan_Campaign objPlan_Campaign = db.Plan_Campaign.Where(pcpobjw => pcpobjw.PlanCampaignId.Equals(id)).SingleOrDefault();
                        verticalId = objPlan_Campaign.VerticalId == null ? string.Empty : objPlan_Campaign.VerticalId.ToString();
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
                        var lstAllowedGeography = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId).ToList();

                        if (GeographyId != string.Empty && verticalId != string.Empty)
                        {
                            if (lstAllowedGeography.Contains(GeographyId) && lstAllowedVertical.Contains(verticalId))
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

            ViewBag.BudinessUnitTitle = db.BusinessUnits.Where(b => b.BusinessUnitId == im.BusinessUnitId).Select(b => b.Title).SingleOrDefault();
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
            ViewBag.IsModelDeploy = im.IntegrationType == "N/A" ? false : true;

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
            var lstAllowedGeography = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId).ToList();
            bool IsTacticEditable = false;
            if (lstAllowedGeography.Contains(pcpt.GeographyId.ToString()) && lstAllowedVertical.Contains(pcpt.VerticalId.ToString()))
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
            bool IsPlanEditOwnAndSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditOwnAndSubordinates);
            //Get all subordinates of current user upto n level
            var lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            lstSubOrdinates.Add(Sessions.User.UserId);
            bool IsDeployToIntegrationVisible = false;
            if (IsPlanEditAllAuthorized && IsBusinessUnitEditable)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                if (IsTacticEditable)
                {
                    IsDeployToIntegrationVisible = true;
                }
            }
            else if (IsPlanEditOwnAndSubordinatesAuthorized)
            {
                if (lstSubOrdinates.Contains(im.OwnerId))
                {
                    if (IsTacticEditable && IsBusinessUnitEditable)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                    {
                        IsDeployToIntegrationVisible = true;
                    }
                }
            }
            ViewBag.IsDeployToIntegrationVisible = IsDeployToIntegrationVisible;

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
                                  Cost = pcpt.Cost,
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
                                  ProjectedStageValue = pcpt.ProjectedStageValue

                              }).SingleOrDefault();

                    imodel.IntegrationType = GetIntegrationTypeTitleByModel(db.Plan_Campaign_Program_Tactic.SingleOrDefault(varT => varT.PlanTacticId == id).TacticType.Model);
                }
                if (section == Convert.ToString(Enums.Section.Program).ToLower())
                {
                    var objPlan_Campaign_Program = db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == id && pcp.IsDeleted == false).FirstOrDefault();

                    if (isStatusChange)     //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                    {
                        int cntSumbitTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == id && pcpt.IsDeleted == false && !pcpt.Status.Equals(statussubmit)).Count();
                        int cntApproveTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == id && pcpt.IsDeleted == false && (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();
                        int cntDeclineTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == id && pcpt.IsDeleted == false && !pcpt.Status.Equals(statusdecline)).Count();
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

                    imodel.IntegrationType = GetIntegrationTypeTitleByModel(objPlan_Campaign_Program.Plan_Campaign.Plan.Model);
                }

                if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                {

                    var objPlan_Campaign = db.Plan_Campaign.Where(pcp => pcp.PlanCampaignId == id && pcp.IsDeleted == false).FirstOrDefault();

                    if (isStatusChange)     //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                    {
                        // Number of program with status is not 'Submitted' 
                        int cntSumbitProgramStatus = db.Plan_Campaign_Program.Where(pcpt => pcpt.PlanCampaignId == id && pcpt.IsDeleted == false && !pcpt.Status.Equals(statussubmit)).Count();
                        // Number of tactic with status is not 'Submitted'
                        int cntSumbitTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.PlanCampaignId == id && pcpt.IsDeleted == false && !pcpt.Status.Equals(statussubmit)).Count();

                        // Number of program with status is not 'Approved', 'in-progress', 'complete'
                        int cntApproveProgramStatus = db.Plan_Campaign_Program.Where(pcpt => pcpt.PlanCampaignId == id && pcpt.IsDeleted == false && (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();
                        // Number of tactic with status is not 'Approved', 'in-progress', 'complete'
                        int cntApproveTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.PlanCampaignId == id && pcpt.IsDeleted == false && (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();

                        // Number of program with status is not 'Declained'
                        int cntDeclineProgramStatus = db.Plan_Campaign_Program.Where(pcpt => pcpt.PlanCampaignId == id && pcpt.IsDeleted == false && !pcpt.Status.Equals(statusdecline)).Count();
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
                    imodel.IntegrationType = GetIntegrationTypeTitleByModel(objPlan_Campaign.Plan.Model);
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

                    imodel.IntegrationType = GetIntegrationTypeTitleByModel(db.Plan_Improvement_Campaign_Program_Tactic.SingleOrDefault(varT => varT.ImprovementPlanTacticId == id).Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model);

                }

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return imodel;
        }

        /// <summary>
        /// Return integration type title by model
        /// </summary>
        /// <param name="objModel"></param>
        /// <returns></returns>
        public string GetIntegrationTypeTitleByModel(Model objModel)
        {
            string returnValue = string.Empty;

            if (objModel.IntegrationInstanceId == null)
                returnValue = "N/A";
            else
            {
                var IntegrationInstance = db.IntegrationInstances.SingleOrDefault(varI => varI.IntegrationInstanceId == objModel.IntegrationInstanceId);
                if (IntegrationInstance == null)
                {
                    returnValue = "N/A";
                }
                else
                {
                    returnValue = IntegrationInstance.IntegrationType.Title;
                }
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
            im.Revenues = Math.Round(tacticList.Where(tl => tl.PlanTacticId == id).Select(tl => tl.ProjectedRevenue).SingleOrDefault(), 1);
            tacticList = Common.ProjectedRevenueCalculateList(tid, true);

            string TitleProjectedStageValue = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
            string TitleCW = Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString();
            string TitleMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            string TitleRevenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();

            im.ProjectedStageValueActual = db.Plan_Campaign_Program_Tactic_Actual.Where(varA => varA.PlanTacticId == id && varA.StageTitle == TitleProjectedStageValue).ToList().Sum(a => a.Actualvalue);
            im.CWsActual = db.Plan_Campaign_Program_Tactic_Actual.Where(varA => varA.PlanTacticId == id && varA.StageTitle == TitleCW).ToList().Sum(a => a.Actualvalue);
            im.RevenuesActual = db.Plan_Campaign_Program_Tactic_Actual.Where(varA => varA.PlanTacticId == id && varA.StageTitle == TitleRevenue).ToList().Sum(a => a.Actualvalue);
            im.MQLsActual = db.Plan_Campaign_Program_Tactic_Actual.Where(varA => varA.PlanTacticId == id && varA.StageTitle == TitleMQL).ToList().Sum(a => a.Actualvalue);


            im.CWs = Math.Round(tacticList.Where(tl => tl.PlanTacticId == id).Select(tl => tl.ProjectedRevenue).SingleOrDefault(), 1);
            string modifiedBy = string.Empty;
            string createdBy = string.Empty;
            DateTime? modifiedDate = null;
            if (db.Plan_Campaign_Program_Tactic_Actual.Where(t => t.PlanTacticId == im.PlanTacticId).Count() > 0)
            {
                modifiedDate = db.Plan_Campaign_Program_Tactic_Actual.Where(t => t.PlanTacticId == im.PlanTacticId).Select(t => t.CreatedDate).FirstOrDefault();
                createdBy = db.Plan_Campaign_Program_Tactic_Actual.Where(t => t.PlanTacticId == im.PlanTacticId).Select(t => t.CreatedBy).FirstOrDefault().ToString();
            }
            else
            {
                if (im.CostActual != 0)
                {
                    modifiedDate = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanTacticId == im.PlanTacticId).Select(t => t.ModifiedDate).FirstOrDefault();
                    createdBy = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanTacticId == im.PlanTacticId).Select(t => t.ModifiedBy).FirstOrDefault().ToString();
                }
            }

            if (modifiedDate != null)
            {
                User objUser = objBDSUserRepository.GetTeamMemberDetails(new Guid(createdBy), Sessions.ApplicationId);
                modifiedBy = string.Format("{0} {1} by {2} {3}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd"), objUser.FirstName, objUser.LastName);
                ViewBag.UpdatedBy = modifiedBy;
            }
            else
            {
                ViewBag.UpdatedBy = null;
            }
            im.MQLs = Common.CalculateMQLTactic(Convert.ToDouble(im.ProjectedStageValue), im.StartDate, im.PlanTacticId, Convert.ToInt32(im.StageId));

            // Modified by dharmraj for implement new formula to calculate ROI, #533
            if (im.Cost > 0)
            {
                im.ROI = (im.Revenues - im.Cost) / im.Cost;
            }
            else
                im.ROI = 0;

            if (im.CostActual > 0)
            {
                im.ROIActual = (im.RevenuesActual - im.CostActual) / im.CostActual;
            }
            else
            {
                im.ROIActual = 0;
            }

            ViewBag.TacticDetail = im;
            bool isValidUser = true;
            //if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            //{
                if (im.OwnerId != Sessions.User.UserId) isValidUser = false;
            //}
            ViewBag.IsValidUser = isValidUser;

            ViewBag.IsModelDeploy = im.IntegrationType == "N/A" ? false : true;
            if (im.LastSyncDate != null)
            {
                ViewBag.LastSync = "Last synced with " + im.IntegrationType + " " + Common.GetFormatedDate(im.LastSyncDate) + ".";
            }
            else
            {
                ViewBag.LastSync = string.Empty;
            }

            // Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
            var objPlanTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(p => p.PlanTacticId == id);
            var lstUserCustomRestriction = Common.GetUserCustomRestriction();
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            var lstAllowedVertical = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId).ToList();
            var lstAllowedGeography = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId).ToList();
            var lstAllowedBusinessUnit = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).Select(r => r.CustomFieldId).ToList();
            //var IsBusinessUnitEditable = Common.IsBusinessUnitEditable(Sessions.BusinessUnitId);    // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            if (lstAllowedGeography.Contains(objPlanTactic.GeographyId.ToString()) && lstAllowedVertical.Contains(objPlanTactic.VerticalId.ToString()) && lstAllowedBusinessUnit.Contains(objPlanTactic.BusinessUnitId.ToString()))//&& IsBusinessUnitEditable)   // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                ViewBag.IsTacticEditable = true;
            }
            else
            {
                ViewBag.IsTacticEditable = false;
            }

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
            if (dtTactic != null)
            {
                //User userName = objBDSUserRepository.GetTeamMemberDetails(dtTactic.CreatedBy, Sessions.ApplicationId);
                //string lstUpdate = string.Format("{0} {1} by {2} {3}", "Last updated", dtTactic.CreatedDate.ToString("MMM dd"), userName.FirstName, userName.LastName);
                var actualData = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpta => pcpta.PlanTacticId == id).Select(pt => new
                {
                    id = pt.PlanTacticId,
                    stageTitle = pt.StageTitle,
                    period = pt.Period,
                    actualValue = pt.Actualvalue
                });
                return Json(actualData, JsonRequestBehavior.AllowGet);
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
        public JsonResult UploadResult(List<InspectActual> tacticactual, string UserId = "")
        {
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
                            ObjectParameter ReturnValue = new ObjectParameter("ReturnValue", typeof(Int64));
                            db.Plan_Campaign_Program_Tactic_ActualDelete(actualResult.PlanTacticId, ReturnValue);
                            Int64 returnValue;
                            Int64 projectedStageValue = 0, mql = 0, cw = 0;
                            double revenue = 0;
                            Int64.TryParse(ReturnValue.Value.ToString(), out returnValue);
                            if (returnValue == 0)
                            {
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
                                                db.SaveChanges();
                                            }
                                        }
                                    }

                                    foreach (var t in tacticactual)
                                    {
                                        Plan_Campaign_Program_Tactic_Actual objpcpta = new Plan_Campaign_Program_Tactic_Actual();
                                        objpcpta.PlanTacticId = t.PlanTacticId;
                                        objpcpta.StageTitle = t.StageTitle;
                                        if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString()) projectedStageValue += t.ActualValue;
                                        if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()) mql += t.ActualValue;
                                        if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString()) cw += t.ActualValue;
                                        if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString()) revenue += t.ActualValue;
                                        objpcpta.Period = t.Period;
                                        objpcpta.Actualvalue = t.ActualValue;
                                        objpcpta.CreatedDate = DateTime.Now;
                                        objpcpta.CreatedBy = Sessions.User.UserId;
                                        db.Entry(objpcpta).State = EntityState.Added;
                                        db.Plan_Campaign_Program_Tactic_Actual.Add(objpcpta);
                                        db.SaveChanges();
                                    }
                                }
                            }

                            Plan_Campaign_Program_Tactic objPCPT = db.Plan_Campaign_Program_Tactic.Where(pt => pt.PlanTacticId == actualResult.PlanTacticId).SingleOrDefault();
                            objPCPT.CostActual = actualResult.TotalCostActual;
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
                                    tactic.Status = status;
                                    tactic.ModifiedBy = Sessions.User.UserId;
                                    tactic.ModifiedDate = DateTime.Now;
                                    db.Entry(tactic).State = EntityState.Modified;
                                    result = db.SaveChanges();
                                    if (result == 1)
                                    {
                                        if (tactic.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
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
                                    var PlanCampaignId = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.PlanProgramId == tactic.PlanProgramId).Select(a => a.PlanCampaignId).Single();
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

                ViewBag.LastSync = "Last synced with " + im.IntegrationType + " " + Common.GetFormatedDate(im.LastSyncDate) + ".";
            }
            else
            {
                ViewBag.LastSync = string.Empty;
            }

            ViewBag.BudinessUnitTitle = db.BusinessUnits.Where(b => b.BusinessUnitId == im.BusinessUnitId).Select(b => b.Title).SingleOrDefault();
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

            ViewBag.IsModelDeploy = im.IntegrationType == "N/A" ? false : true;

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
            bool IsPlanEditOwnAndSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditOwnAndSubordinates);
            //Get all subordinates of current user upto n level
            var lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            lstSubOrdinates.Add(Sessions.User.UserId);
            bool IsProgramEditable = false;
            if (IsPlanEditAllAuthorized && IsBusinessUnitEditable)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                IsProgramEditable = true;
            }
            else if (IsPlanEditOwnAndSubordinatesAuthorized)
            {
                if (lstSubOrdinates.Contains(im.OwnerId) && IsBusinessUnitEditable) // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsProgramEditable = true;
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

                ViewBag.LastSync = "Last synced with " + im.IntegrationType + " " + Common.GetFormatedDate(im.LastSyncDate) + ".";
            }
            else
            {
                ViewBag.LastSync = string.Empty;
            }

            ViewBag.CampaignDetail = im;
            ViewBag.BudinessUnitTitle = db.BusinessUnits.Where(b => b.BusinessUnitId == im.BusinessUnitId).Select(b => b.Title).SingleOrDefault();
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

            ViewBag.IsModelDeploy = im.IntegrationType == "N/A" ? false : true;

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
            bool IsPlanEditOwnAndSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditOwnAndSubordinates);
            //Get all subordinates of current user upto n level
            var lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            lstSubOrdinates.Add(Sessions.User.UserId);
            bool IsCampaignEditable = false;
            if (IsPlanEditAllAuthorized && IsBusinessUnitEditable)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                IsCampaignEditable = true;
            }
            else if (IsPlanEditOwnAndSubordinatesAuthorized)
            {
                if (lstSubOrdinates.Contains(im.OwnerId) && IsBusinessUnitEditable) // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsCampaignEditable = true;
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
                    if (strparam == Enums.UpcomingActivities.planYear.ToString() || strparam == "")
                    {
                        year = Convert.ToInt32(objPlan_Campaign_Program_Tactic[0].Plan_Campaign_Program.Plan_Campaign.Plan.Year);
                    }
                    else if (strparam == Enums.UpcomingActivities.thisyear.ToString())
                    {
                        year = DateTime.Now.Year;
                    }
                    else if (strparam == Enums.UpcomingActivities.nextyear.ToString())
                    {
                        year = DateTime.Now.Year + 1;
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
                                        end = new DateTime(start.Year, start.Month, 31);
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
                    end = new DateTime(start.Year, start.Month, 31);
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
                int type = 0; // this is inititalized as 0 bcoz to get the status for tactics.
                var individuals = GetIndividualsByPlanId(Sessions.PlanId, (GanttTabs)type, Enums.ActiveMenu.Home.ToString());
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
                lstOwnAndSubOrdinates.Add(Sessions.User.UserId);
                bool IsPlanEditable = true;
                bool IsPlanEditOwnAndSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditOwnAndSubordinates);
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                var objPlan = db.Plans.FirstOrDefault(p => p.PlanId == Sessions.PlanId);
                if (IsPlanEditAllAuthorized)
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditOwnAndSubordinatesAuthorized)
                {
                    if (lstOwnAndSubOrdinates.Contains(objPlan.CreatedBy))
                    {
                        IsPlanEditable = true;
                    }
                    else
                    {
                        IsPlanEditable = false;
                    }
                }
                else
                {
                    IsPlanEditable = false;
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
            var lstAllowedGeography = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId).ToList();
            TacticList = TacticList.Where(t => lstAllowedVertical.Contains(t.VerticalId.ToString()) && lstAllowedGeography.Contains(t.GeographyId.ToString())).ToList();

            List<int> TacticIds = TacticList.Select(t => t.PlanTacticId).ToList();
            var dtTactic = (from pt in db.Plan_Campaign_Program_Tactic_Actual
                            where TacticIds.Contains(pt.PlanTacticId)
                            select new { pt.PlanTacticId, pt.CreatedBy, pt.CreatedDate }).GroupBy(pt => pt.PlanTacticId).Select(pt => pt.FirstOrDefault());
            List<Guid> userListId = new List<Guid>();
            userListId = (from ta in dtTactic select ta.CreatedBy).ToList<Guid>();

            // Added BY Bhavesh
            // Calculate MQL at runtime #376
            List<Plan_Tactic_MQL> MQLTacticList = Common.GetMQLValueTacticList(TacticList);
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
            var lstEditableGeography = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId).ToList();

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
                costProjected = t.Cost,
                costActual = t.CostActual == null ? 0 : t.CostActual,
                roiProjected = 0,//t.ROI == null ? 0 : t.ROI,
                roiActual = 0,//t.ROIActual == null ? 0 : t.ROIActual,
                geographyId = t.GeographyId,
                individualId = t.CreatedBy,
                tacticTypeId = t.TacticTypeId,
                modifiedBy = string.Format("{0} {1} by {2} {3}", "Last updated", Convert.ToDateTime(t.ModifiedDate).ToString("MMM dd"), userName.Where(u => u.UserId == t.CreatedBy).Select(u => u.FirstName).FirstOrDefault(), userName.Where(u => u.UserId == t.CreatedBy).Select(u => u.LastName).FirstOrDefault()),
                actualData = (db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pct => pct.PlanTacticId.Equals(t.PlanTacticId)).Select(pcp => pcp).ToList()).Select(pcpt => new
                {
                    title = pcpt.StageTitle,
                    period = pcpt.Period,
                    actualValue = pcpt.Actualvalue,
                    UpdateBy = string.Format("{0} {1} by {2} {3}", "Last updated", pcpt.CreatedDate.ToString("MMM dd"), userName.Where(u => u.UserId == pcpt.CreatedBy).Select(u => u.FirstName).FirstOrDefault(), userName.Where(u => u.UserId == pcpt.CreatedBy).Select(u => u.LastName).FirstOrDefault()),
                    IsUpdate = status
                }).Select(pcp => pcp).Distinct(),
                LastSync = t.LastSyncDate == null ? string.Empty : ("Last synced with " + GetIntegrationTypeTitleByModel(t.TacticType.Model) + " " + Common.GetFormatedDate(t.LastSyncDate) + "."),
                stageTitle = Common.GetTacticStageTitle(t.PlanTacticId),
                tacticStageId = t.Stage.StageId,
                tacticStageTitle = t.Stage.Title,
                projectedStageValue = t.ProjectedStageValue,
                projectedStageValueActual = lstTacticActual.Where(a => a.PlanTacticId == t.PlanTacticId && a.StageTitle == TitleProjectedStageValue).Sum(a => a.Actualvalue),
                IsTacticEditable = (lstEditableGeography.Contains(t.GeographyId.ToString()) && lstEditableVertical.Contains(t.VerticalId.ToString()))
            }).Select(t => t).Distinct().OrderBy(t => t.id);

            var lstTactic = tacticObj.Select(t => new
            {
                id = t.id,
                title = t.title,
                mqlProjected = t.mqlProjected,
                mqlActual = t.mqlActual,
                cwProjected = t.cwProjected,
                cwActual = t.cwActual,
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
                IsTacticEditable = t.IsTacticEditable
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


            ViewBag.IsModelDeploy = im.IntegrationType == "N/A" ? false : true;
            if (im.LastSyncDate != null)
            {
                TimeZone localZone = TimeZone.CurrentTimeZone;

                ViewBag.LastSync = "Last synced with " + im.IntegrationType + " " + Common.GetFormatedDate(im.LastSyncDate) + ".";
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

            ViewBag.IsModelDeploy = im.IntegrationType == "N/A" ? false : true;

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
            bool IsPlanEditOwnAndSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditOwnAndSubordinates);
            //Get all subordinates of current user upto n level
            var lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            lstSubOrdinates.Add(Sessions.User.UserId);
            bool IsDeployToIntegrationVisible = false;
            if (IsPlanEditAllAuthorized && IsBusinessUnitEditable)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                IsDeployToIntegrationVisible = true;
            }
            else if (IsPlanEditOwnAndSubordinatesAuthorized)
            {
                if (lstSubOrdinates.Contains(im.OwnerId) && IsBusinessUnitEditable) // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsDeployToIntegrationVisible = true;
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

        #region Get Individuals by planID Method
        /// <summary>
        /// Added By :- Sohel Pathan
        /// Date :- 14/04/2014
        /// Reason :- To get list of users who have created tactic by planId (PL ticket # 428)
        /// </summary>
        /// <param name="PlanId"></param>
        /// <returns></returns>
        public JsonResult GetIndividualsForFilter(int PlanId, int type, string activeMenu)
        {
            var individuals = GetIndividualsByPlanId(PlanId, (GanttTabs)type, activeMenu); //// Modified by :- Sohel Pathan on 17/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
            individuals = individuals.Select(a => new User { UserId = a.UserId, FirstName = a.FirstName, LastName = a.LastName }).ToList();
            return Json(new { individualsList = individuals.OrderBy(i => string.Format("{0} {1}", i.FirstName, i.LastName)).ToList() }, JsonRequestBehavior.AllowGet);
        }

        private List<User> GetIndividualsByPlanId(int PlanId, GanttTabs type, string activeMenu)
        {
            BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();
            //// Added by :- Sohel Pathan on 17/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, activeMenu.ToLower());
            List<string> status = GetStatusAsPerTab(type, objactivemenu);
            ////

            //// Added by :- Sohel Pathan on 21/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
            if (activeMenu.Equals(Enums.ActiveMenu.Plan.ToString()))
            {
                List<string> statusCD = new List<string>();
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

                var TacticUserList = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted.Equals(false) && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId == PlanId).Distinct().ToList();
                TacticUserList = TacticUserList.Where(t => status.Contains(t.Status) || ((t.CreatedBy == Sessions.User.UserId && !type.Equals(GanttTabs.Request)) ? statusCD.Contains(t.Status) : false)).Distinct().ToList();

                var ImprovementTacticUserList = db.Plan_Improvement_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted.Equals(false) && pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId == PlanId).Distinct().ToList();
                ImprovementTacticUserList = ImprovementTacticUserList.Where(t => status.Contains(t.Status) || ((t.CreatedBy == Sessions.User.UserId && !type.Equals(GanttTabs.Request)) ? statusCD.Contains(t.Status) : false)).Distinct().ToList();

                var mergedList = TacticUserList.Select(a => a.CreatedBy).ToList();
                mergedList.AddRange(ImprovementTacticUserList.Select(a => a.CreatedBy).ToList());
                mergedList = mergedList.Distinct().ToList();

                string strContatedIndividualList = string.Empty;
                foreach (var item in mergedList)
                {
                    strContatedIndividualList += item.ToString() + ',';
                }
                var individuals = bdsUserRepository.GetMultipleTeamMemberDetails(strContatedIndividualList.TrimEnd(','), Sessions.ApplicationId);

                return individuals;
            }
            else
            {
                //// Modified by :- Sohel Pathan on 17/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
                var TacticUserList = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted.Equals(false) && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId == PlanId && status.Contains(pcpt.Status)).Select(a => a.CreatedBy).Distinct().ToList();
                var ImprovementTacticUserList = db.Plan_Improvement_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted.Equals(false) && pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId == PlanId && status.Contains(pcpt.Status)).Select(a => a.CreatedBy).Distinct().ToList();
                ////
                TacticUserList.AddRange(ImprovementTacticUserList);
                TacticUserList = TacticUserList.Distinct().ToList();
                string strContatedIndividualList = string.Empty;
                foreach (var item in TacticUserList)
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

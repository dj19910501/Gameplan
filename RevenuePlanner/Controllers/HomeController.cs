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
            if (Sessions.RolePermission != null)
            {
                Common.Permission permission = Common.GetPermission(ActionItem.Home);
                switch (permission)
                {
                    case Common.Permission.FullAccess:
                        break;
                    case Common.Permission.NoAccess:
                        return RedirectToAction("Homezero", "Home");
                    case Common.Permission.NotAnEntity:
                        break;
                    case Common.Permission.ViewOnly:
                        ViewBag.IsViewOnly = "true";
                        break;
                }
            }

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

            if (Sessions.IsSystemAdmin)
            {
                var clientBusinessUnit = db.BusinessUnits.Where(b => b.IsDeleted == false).Select(b => b.BusinessUnitId).ToList<Guid>();
                businessUnitIds = clientBusinessUnit.ToList();
                planmodel.BusinessUnitIds = Common.GetBussinessUnitIds(Sessions.User.ClientId); //commented due to not used any where
                ViewBag.showBid = true;
            }
            else if (Sessions.IsDirector || Sessions.IsClientAdmin)
            {
                var clientBusinessUnit = db.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId) && b.IsDeleted == false).Select(b => b.BusinessUnitId).ToList<Guid>();
                businessUnitIds = clientBusinessUnit.ToList();
                planmodel.BusinessUnitIds = Common.GetBussinessUnitIds(Sessions.User.ClientId); //commented due to not used any where
                ViewBag.showBid = true;
            }
            else
            {
                businessUnitIds.Add(Sessions.User.BusinessUnitId);
                ViewBag.showBid = false;
            }

            //// Getting active model of above business unit. 
            string modelPublishedStatus = Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value;
            var models = db.Models.Where(m => businessUnitIds.Contains(m.BusinessUnitId) && m.IsDeleted == false).Select(m => m);

            //// Getting modelIds
            var modelIds = models.Select(m => m.ModelId).ToList();

            var activePlan = db.Plans.Where(p => modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true) && p.IsDeleted == false).Select(p => p);

            if (Enums.ActiveMenu.Home.Equals(activeMenu))
            {
                //// Getting Active plan for all above models.
                string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
                activePlan = activePlan.Where(p => p.Status.Equals(planPublishedStatus));
            }

            if (activePlan.Count() != 0)
            {
                try
                {
                    Plan currentPlan = new Plan();
                    if (currentPlanId != 0)
                    {
                        currentPlan = activePlan.Where(p => p.PlanId.Equals(currentPlanId)).Select(p => p).FirstOrDefault();
                    }
                    else if (!Common.IsPlanPublished(Sessions.PlanId))
                    {
                        /* added by Nirav shah for TFS Point : 218*/
                        if (Sessions.PublishedPlanId == 0)
                        {
                            currentPlan = activePlan.Select(p => p).FirstOrDefault();
                        }
                        else
                        {
                            currentPlan = activePlan.Where(p => p.PlanId.Equals(Sessions.PublishedPlanId)).Select(p => p).FirstOrDefault();
                        }
                    }
                    else
                    {
                        /* added by Nirav shah for TFS Point : 218*/
                        if (Sessions.PlanId == 0)
                        {
                            currentPlan = activePlan.Select(p => p).FirstOrDefault();
                        }
                        else
                        {
                            currentPlan = activePlan.Where(p => p.PlanId.Equals(Sessions.PlanId)).Select(p => p).FirstOrDefault();
                        }
                    }

                    planmodel.PlanTitle = currentPlan.Title;
                    planmodel.PlanId = currentPlan.PlanId;
                    planmodel.objplanhomemodelheader = Common.GetPlanHeaderValue(currentPlan.PlanId);

                    //List<SelectListItem> planList = Common.GetPlan().Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).Take(5).ToList();

                    List<SelectListItem> UpcomingActivityList = Common.GetUpcomingActivity().Select(p => new SelectListItem() { Text = p.Text, Value = p.Value.ToString() }).ToList();
                    planmodel.objplanhomemodelheader.UpcomingActivity = UpcomingActivityList;

                    // planList.Single(p => p.Value.Equals(currentPlan.PlanId.ToString())).Selected = true;
                    // planmodel.objHomePlan.plans = planList;

                    //Start Maninder Singh Wadhva : 11/25/2013 - Getting list of geographies and individuals.
                    planmodel.objGeography = db.Geographies.Where(g => g.IsDeleted.Equals(false) && g.ClientId.Equals(Sessions.User.ClientId)).Select(g => g).OrderBy(g => g.Title).ToList();

                    BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();
                    var individuals = bdsUserRepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, Sessions.IsSystemAdmin);
                    planmodel.objIndividuals = individuals.OrderBy(i => string.Format("{0} {1}", i.FirstName, i.LastName)).ToList();
                    //End Maninder Singh Wadhva : 11/25/2013 - Getting list of geographies and individuals.

                    Sessions.PlanId = planmodel.PlanId;

                    planmodel.CollaboratorId = GetCollaborator(currentPlan);

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
                return View(planmodel);
            }
            else
            {
                TempData["ErrorMessage"] = "No plan available, please create a plan";
                return RedirectToAction("PlanSelector", "Plan");
            }

        }
        //New parameter activeMenu added to check whether this method is called from Home or Plan
        public ActionResult HomePlan(string Bid, int currentPlanId, string activeMenu)
        {
            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, activeMenu.ToLower());
            HomePlan objHomePlan = new HomePlan();
            objHomePlan.IsDirector = Sessions.IsDirector;
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
            }
            objHomePlan.plans = planList;
            return PartialView("_PlanDropdown", objHomePlan);
        }
        #endregion

        #region Get Collaborator Details

        /// <summary>
        /// Get Collaborator Details for current plan.
        /// </summary>
        /// <param name="currentPlanId">PlanId</param>
        /// <returns>Json Result.</returns>
        public JsonResult GetCollaboratorDetails(int currentPlanId)
        {
            var plan = db.Plans.Single(p => p.PlanId.Equals(currentPlanId));

            List<string> collaboratorId = new List<string>();
            if (plan.ModifiedBy != null)
            {
                collaboratorId.Add(plan.ModifiedBy.ToString());
            }

            if (plan.CreatedBy != null)
            {
                collaboratorId.Add(plan.CreatedBy.ToString());
            }

            var planTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(t => t);

            var planTacticModifiedBy = planTactic.ToList().Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
            var planTacticCreatedBy = planTactic.ToList().Select(t => t.CreatedBy.ToString()).ToList();

            var planProgramModifiedBy = planTactic.ToList().Where(t => t.Plan_Campaign_Program.ModifiedBy != null).Select(t => t.Plan_Campaign_Program.ModifiedBy.ToString()).ToList();
            var planProgramCreatedBy = planTactic.ToList().Select(t => t.Plan_Campaign_Program.CreatedBy.ToString()).ToList();

            var planCampaignModifiedBy = planTactic.ToList().Where(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedBy != null).Select(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedBy.ToString()).ToList();
            var planCampaignCreatedBy = planTactic.ToList().Select(t => t.Plan_Campaign_Program.Plan_Campaign.CreatedBy.ToString()).ToList();

            var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId))
                                                                           .Select(pc => pc);
            var planTacticCommentCreatedBy = planTacticComment.ToList().Select(pc => pc.CreatedBy.ToString()).ToList();

            collaboratorId.AddRange(planTacticCreatedBy);
            collaboratorId.AddRange(planTacticModifiedBy);
            collaboratorId.AddRange(planProgramCreatedBy);
            collaboratorId.AddRange(planProgramModifiedBy);
            collaboratorId.AddRange(planCampaignCreatedBy);
            collaboratorId.AddRange(planCampaignModifiedBy);
            collaboratorId.AddRange(planTacticCommentCreatedBy);

            return Json(new
            {
                collaboratorList = string.Join(",", collaboratorId.Distinct().ToList<string>()),
                lastUpdateDate = String.Format("{0:g}", GetLastUpdatedDate(plan))
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Collaborator image.
        /// </summary>
        /// <param name="collaboratorId">Collaborator Id</param>
        /// <returns>Json Result.</returns>
        [HttpGet]
        public ActionResult GetCollaboratorImage(string collaboratorId)
        {
            Guid userId = new Guid();
            byte[] imageBytes = Common.ReadFile(Server.MapPath("~") + "/content/images/user_image_not_found.png");
            try
            {
                if (collaboratorId != null)
                {
                    userId = Guid.Parse(collaboratorId);
                    BDSService.User objUser = new BDSService.User();
                    objUser = objBDSUserRepository.GetTeamMemberDetails(userId, Sessions.ApplicationId);
                    if (objUser != null)
                    {
                        if (objUser.ProfilePhoto != null)
                        {
                            imageBytes = objUser.ProfilePhoto;
                        }
                    }
                }
                if (imageBytes != null)
                {
                    MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                    image = Common.ImageResize(image, 30, 30, true, false);
                    imageBytes = Common.ImageToByteArray(image);
                }
            }
            catch (Exception e)
            {
                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                    imageBytes = Common.ReadFile(Server.MapPath("~") + "/content/images/user_image_not_found.png");
                    MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                    image = Common.ImageResize(image, 30, 30, true, false);
                    imageBytes = Common.ImageToByteArray(image);
                }
            }

            return Json(new { base64image = Convert.ToBase64String(imageBytes) }
                , JsonRequestBehavior.AllowGet);
        }

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

        #endregion

        #region "Getting list of collaborator for current plan"

        /// <summary>
        /// Getting list of collaborator for current plan.
        /// </summary>
        /// <param name="plan">Plan</param>
        /// <returns>Returns list of collaborators for current plan.</returns>
        private List<string> GetCollaborator(Plan plan)
        {
            List<string> collaboratorId = new List<string>();
            if (plan.ModifiedBy != null)
            {
                collaboratorId.Add(plan.ModifiedBy.ToString());
            }

            if (plan.CreatedBy != null)
            {
                collaboratorId.Add(plan.CreatedBy.ToString());
            }

            var planTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(t => t);

            var planTacticModifiedBy = planTactic.ToList().Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
            var planTacticCreatedBy = planTactic.ToList().Select(t => t.CreatedBy.ToString()).ToList();

            var planProgramModifiedBy = planTactic.ToList().Where(t => t.Plan_Campaign_Program.ModifiedBy != null).Select(t => t.Plan_Campaign_Program.ModifiedBy.ToString()).ToList();
            var planProgramCreatedBy = planTactic.ToList().Select(t => t.Plan_Campaign_Program.CreatedBy.ToString()).ToList();

            var planCampaignModifiedBy = planTactic.ToList().Where(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedBy != null).Select(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedBy.ToString()).ToList();
            var planCampaignCreatedBy = planTactic.ToList().Select(t => t.Plan_Campaign_Program.Plan_Campaign.CreatedBy.ToString()).ToList();

            var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId))
                                                                           .Select(pc => pc);
            var planTacticCommentCreatedBy = planTacticComment.ToList().Select(pc => pc.CreatedBy.ToString()).ToList();

            collaboratorId.AddRange(planTacticCreatedBy);
            collaboratorId.AddRange(planTacticModifiedBy);
            collaboratorId.AddRange(planProgramCreatedBy);
            collaboratorId.AddRange(planProgramModifiedBy);
            collaboratorId.AddRange(planCampaignCreatedBy);
            collaboratorId.AddRange(planCampaignModifiedBy);
            collaboratorId.AddRange(planTacticCommentCreatedBy);
            return collaboratorId.Distinct().ToList<string>();
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
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            image = Common.ImageResize(image, 36, 36, true, false);
            imageBytes = Common.ImageToByteArray(image);
            return File(imageBytes, "image/jpg");
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
            List<string> filterGeography = string.IsNullOrWhiteSpace(geographyIds) ? new List<string>() : geographyIds.Split(',').ToList();

            //// Individual filter criteria.
            List<string> filterIndividual = string.IsNullOrWhiteSpace(individualsIds) ? new List<string>() : individualsIds.Split(',').ToList();

            if (isShowOnlyMyTactic)
            {
                filterIndividual.Add(Sessions.User.UserId.ToString());
            }

            var objPlan_Campaign_Program_Tactic_Comment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.PlanTacticId != null).ToList()
                                                        .Where(pcptc => pcptc.Plan_Campaign_Program_Tactic.IsDeleted.Equals(false) &&
                                                            programId.Contains(pcptc.Plan_Campaign_Program_Tactic.PlanProgramId) &&
                                                            //// Individual & Show my Tactic Filter 
                                                           (filterIndividual.Count.Equals(0) || filterIndividual.Contains(pcptc.CreatedBy.ToString()))).ToList();

            //// Applying filters to tactic comment (IsDelete, Individuals and Show My Tactic)
            List<int?> tacticId = objPlan_Campaign_Program_Tactic_Comment
                                                        .Select(pcptc => pcptc.PlanTacticId).ToList();

            //// Applying filters to tactic (IsDelete, Geography, Individuals and Show My Tactic)
            var tactic = db.Plan_Campaign_Program_Tactic.ToList()
                                                        .Where(pcpt => pcpt.IsDeleted.Equals(false) &&
                                                                       programId.Contains(pcpt.PlanProgramId) &&
                                                            //// Checking start and end date
                                                        Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                    CalendarEndDate,
                                                                                                    pcpt.StartDate,
                                                                                                    pcpt.EndDate).Equals(false) &&
                                                            //// Geography Filter
                                                                       (filterGeography.Count.Equals(0) || filterGeography.Contains(pcpt.GeographyId.ToString())) &&
                                                            //// Individual & Show my Tactic Filter 
                                                                       (filterIndividual.Count.Equals(0) || filterIndividual.Contains(pcpt.CreatedBy.ToString()) || filterIndividual.Contains(pcpt.ModifiedBy.ToString()) || tacticId.Contains(pcpt.PlanTacticId)))
                                                        .Select(pcpt => pcpt);

            //// Modified By Maninder Singh Wadhva PL Ticket#47
            List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(improveTactic => improveTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(planId) &&
                                                                                      improveTactic.IsDeleted.Equals(false) &&
                                                                                      (improveTactic.EffectiveDate > CalendarEndDate).Equals(false))
                                                                               .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();
            string tacticStatus = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            //// Modified By Maninder Singh Wadhva PL Ticket#47
            string requestCount = Convert.ToString(tactic.Where(t => t.Status.Equals(tacticStatus)).Count() + improvementTactic.Where(improveTactic => improveTactic.Status.Equals(tacticStatus)).Count());

            GanttTabs currentGanttTab = (GanttTabs)type;

            //// Modified By Maninder Singh Wadhva PL Ticket#47
            //// Show submitted/apporved/in-progress/complete improvement tactic.
            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, activeMenu.ToLower());
            if (objactivemenu.Equals(Enums.ActiveMenu.Home))
            {
                List<string> status = GetStatusAsPerTab(currentGanttTab);
                improvementTactic = improvementTactic.Where(improveTactic => status.Contains(improveTactic.Status))
                                                       .Select(improveTactic => improveTactic)
                                                       .ToList<Plan_Improvement_Campaign_Program_Tactic>();
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
                        TaskId = string.Format("C{0}_P{1}_T{2}_Y{3}", pcpt.Plan_Campaign_Program.PlanCampaignId, pcpt.PlanProgramId, pcpt.PlanTacticId, pcpt.TacticTypeId)
                    });

                    var tacticType = (tactic.Select(pcpt => pcpt.TacticType)).Select(pcpt => new
                    {
                        pcpt.TacticTypeId,
                        pcpt.Title,
                        ColorCode = pcpt.ColorCode
                    }).Distinct().OrderBy(pcpt => pcpt.Title);

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    List<object> tacticAndRequestTaskData = GetTaskDetailTactic(tactic.ToList());
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
                                TaskId = string.Format("V{0}_C{1}_P{2}_T{3}", pcpt.VerticalId, pcpt.Plan_Campaign_Program.PlanCampaignId, pcpt.PlanProgramId, pcpt.PlanTacticId)
                            });

                    var verticals = (tactic.Select(vertical => vertical.Vertical)).Select(vertical => new
                    {
                        VerticalId = vertical.VerticalId,
                        Title = vertical.Title,
                        ColorCode = vertical.ColorCode
                    }).Distinct().OrderBy(vertical => vertical.Title);

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    List<object> verticalTaskData = GetTaskDetailVertical(campaign.ToList(), program.ToList(), tactic.ToList());
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
                    var queryStages = tactic.Select(t => new
                    {
                        t.TacticType.StageId,
                        t.TacticType.Stage.Title,
                        t.TacticType.Stage.ColorCode
                    }).Distinct();

                    var queryStageTacticType = tactic.Select(t => new
                    {
                        t.PlanTacticId,
                        t.Title,
                        t.TacticType.StageId,
                        TaskId = string.Format("S{0}_C{1}_P{2}_T{3}", t.TacticType.StageId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId, t.PlanTacticId)
                    });

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    List<object> stageTaskData = GetTaskDetailStage(campaign.ToList(), program.ToList(), tactic.ToList());
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
                               TaskId = string.Format("A{0}_C{1}_P{2}_T{3}", pcpt.AudienceId, pcpt.Plan_Campaign_Program.PlanCampaignId, pcpt.PlanProgramId, pcpt.PlanTacticId)
                           });

                    var audiences = (tactic.Select(audience => audience.Audience)).Select(audience => new
                    {
                        audience.AudienceId,
                        audience.Title,
                        ColorCode = audience.ColorCode
                    }).Distinct().OrderBy(audience => audience.Title);

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    List<object> audienceTaskData = GetTaskDetailAudience(campaign.ToList(), program.ToList(), tactic.ToList());
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
                        TaskId = string.Format("B{0}_C{1}_P{2}_T{3}", businessUnit.BusinessUnitId, pt.Plan_Campaign_Program.PlanCampaignId, pt.PlanProgramId, pt.PlanTacticId)
                    });

                    //// Modified By Maninder Singh Wadhva PL Ticket#47
                    List<object> businessUnitTaskData = GetTaskDetailBusinessUnit(businessUnit, tactic.ToList());
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
        /// Added By Maninder Singh Wadhva PL Ticket#47
        /// Function to get status as per tab.
        /// </summary>
        /// <param name="currentTab">Current Tab.</param>
        /// <returns>Returns list of status as per tab.</returns>
        private List<string> GetStatusAsPerTab(GanttTabs currentTab)
        {
            List<string> status = Common.GetStatusListAfterApproved();

            if (currentTab.Equals(GanttTabs.Request))
            {
                status.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
                status.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());
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
            //// Getting plan improvement tactic for left accordion.
            var improvementPlanTactic = improvementTactics.Select(improvementTactic => new
            {
                improvementTactic.ImprovementPlanTacticId,
                improvementTactic.ImprovementTacticTypeId,
                improvementTactic.Title,
                TaskId = string.Format("M{0}_I{1}_Y{2}", 1, improvementTactic.ImprovementPlanTacticId, improvementTactic.ImprovementTacticTypeId)
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
        private List<object> GetTaskDetailBusinessUnit(BusinessUnit businessUnit, List<Plan_Campaign_Program_Tactic> planTactic)
        {
            //// Business Unit.
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
                progress = 0,
                open = false,
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, businessUnit.ColorCode.ToLower())
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
                progress = 0,
                open = false,
                parent = string.Format("B{0}_C{1}_P{2}", businessUnit.BusinessUnitId, bt.Plan_Campaign_Program.PlanCampaignId, bt.Plan_Campaign_Program.PlanProgramId),
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, businessUnit.ColorCode.ToLower()),
                plantacticid = bt.PlanTacticId
            }).OrderBy(t => t.text);

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
                progress = 0,
                open = false,
                parent = string.Format("B{0}_C{1}", businessUnit.BusinessUnitId, bt.Plan_Campaign_Program.PlanCampaignId),
                color = "",
                planprogramid = bt.Plan_Campaign_Program.PlanProgramId
            }).Select(p => p).Distinct().OrderBy(p => p.text);

            //// Campaign
            var taskDataCampaign = planTactic.Select(bt => new
            {
                id = string.Format("B{0}_C{1}", businessUnit.BusinessUnitId, bt.Plan_Campaign_Program.PlanCampaignId),
                text = bt.Plan_Campaign_Program.Plan_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, bt.Plan_Campaign_Program.Plan_Campaign.StartDate),
                duration = (bt.Plan_Campaign_Program.Plan_Campaign.EndDate - bt.Plan_Campaign_Program.Plan_Campaign.StartDate).TotalDays,
                progress = 0,
                open = false,
                parent = string.Format("B{0}", businessUnit.BusinessUnitId),
                color = Common.COLORC6EBF3_WITH_BORDER,
                plancampaignid = bt.Plan_Campaign_Program.PlanCampaignId
            }).Select(c => c).Distinct().OrderBy(c => c.text);

            return taskDataCampaign.Concat<object>(taskDataProgram).Concat<object>(taskDataTactic).Concat(new[] { queryBusinessUnitTacticType }).ToList<object>();
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
        public List<object> GetTaskDetailAudience(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
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
                progress = 0,
                open = false,
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Audience.ColorCode.ToLower())
            }).Select(a => a).Distinct().OrderBy(t => t.text);

            var taskDataCampaign = tactic.Select(t => new
            {
                id = string.Format("A{0}_C{1}", t.AudienceId, t.Plan_Campaign_Program.PlanCampaignId),
                text = t.Plan_Campaign_Program.Plan_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.Plan_Campaign.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.Plan_Campaign_Program.Plan_Campaign.StartDate,
                                                          t.Plan_Campaign_Program.Plan_Campaign.EndDate),
                progress = 0,
                open = false,
                parent = string.Format("A{0}", t.AudienceId),
                color = Common.COLORC6EBF3_WITH_BORDER,
                plancampaignid = t.Plan_Campaign_Program.PlanCampaignId
            }).Select(t => t).Distinct().OrderBy(t => t.text);

            var taskDataProgram = tactic.Select(t => new
            {
                id = string.Format("A{0}_C{1}_P{2}", t.AudienceId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),
                text = t.Plan_Campaign_Program.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.Plan_Campaign_Program.StartDate,
                                                          t.Plan_Campaign_Program.EndDate),
                progress = 0,
                open = false,
                parent = string.Format("A{0}_C{1}", t.AudienceId, t.Plan_Campaign_Program.PlanCampaignId),
                color = "",
                planprogramid = t.PlanProgramId
            }).Select(t => t).Distinct().OrderBy(t => t.text);

            var taskDataTactic = tactic.Select(t => new
            {
                id = string.Format("A{0}_C{1}_P{2}_T{3}", t.AudienceId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId),
                text = t.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.StartDate,
                                                          t.EndDate),
                progress = 0,
                open = false,
                parent = string.Format("A{0}_C{1}_P{2}", t.AudienceId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Audience.ColorCode.ToLower()),
                plantacticid = t.PlanTacticId
            }).OrderBy(t => t.text);

            return taskDataAudience.Concat<object>(taskDataCampaign).Concat<object>(taskDataTactic).Concat<object>(taskDataProgram).ToList<object>();
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/22/2013
        /// Function to get GANTT chart task detail for Stage.
        /// Modfied By: Maninder Singh Wadhva.
        /// Reason: To show task whose either start or end date or both date are outside current view.
        /// </summary>
        /// <param name="campaign">Campaign of plan version.</param>
        /// <param name="program">Program of plan version.</param>
        /// <param name="tactic">Tactic of plan version.</param>
        /// <returns>Returns list of task for GANNT CHART.</returns>
        public List<object> GetTaskDetailStage(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
        {
            //// Stage
            var taskStages = tactic.Select(t => new
            {
                id = string.Format("S{0}", t.TacticType.StageId),
                text = t.TacticType.Stage.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.TacticType.StageId.Equals(t.TacticType.StageId)).ToList<Plan_Campaign_Program_Tactic>())),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          GetMinStartDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.TacticType.StageId.Equals(t.TacticType.StageId)).ToList<Plan_Campaign_Program_Tactic>()),
                                                          GetMaxEndDateStageAndBusinessUnit(campaign, program, tactic.Where(tt => tt.TacticType.StageId.Equals(t.TacticType.StageId)).ToList<Plan_Campaign_Program_Tactic>())),
                progress = 0,
                open = false,
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.TacticType.Stage.ColorCode.ToLower())
            }).Distinct();

            //// Tactic
            var taskDataTactic = tactic.Select(t => new
            {
                id = string.Format("S{0}_C{1}_P{2}_T{3}", t.TacticType.StageId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId),
                text = t.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.StartDate,
                                                          t.EndDate),
                progress = 0,
                open = false,
                parent = string.Format("S{0}_C{1}_P{2}", t.TacticType.StageId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.TacticType.Stage.ColorCode.ToLower()),
                plantacticid = t.PlanTacticId
            }).OrderBy(t => t.text);

            //// Program
            var taskDataProgram = tactic.Select(t => new
            {
                id = string.Format("S{0}_C{1}_P{2}", t.TacticType.StageId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),
                text = t.Plan_Campaign_Program.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.Plan_Campaign_Program.StartDate,
                                                          t.Plan_Campaign_Program.EndDate),
                progress = 0,
                open = false,
                parent = string.Format("S{0}_C{1}", t.TacticType.StageId, t.Plan_Campaign_Program.PlanCampaignId),
                color = "",
                planprogramid = t.PlanProgramId
            }).Select(p => p).Distinct().OrderBy(p => p.text);

            //// Campaign
            var taskDataCampaign = tactic.Select(t => new
            {
                id = string.Format("S{0}_C{1}", t.TacticType.StageId, t.Plan_Campaign_Program.PlanCampaignId),
                text = t.Plan_Campaign_Program.Plan_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.Plan_Campaign.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.Plan_Campaign_Program.Plan_Campaign.StartDate,
                                                          t.Plan_Campaign_Program.Plan_Campaign.EndDate),
                progress = 0,
                open = false,
                parent = string.Format("S{0}", t.TacticType.StageId),
                color = Common.COLORC6EBF3_WITH_BORDER,
                plancampaignid = t.Plan_Campaign_Program.PlanCampaignId
            }).Select(c => c).Distinct().OrderBy(c => c.text);

            return taskStages.Concat<object>(taskDataCampaign).Concat<object>(taskDataProgram).Concat<object>(taskDataTactic).ToList<object>();
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
        public List<object> GetTaskDetailVertical(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
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
                progress = 0,
                open = false,
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Vertical.ColorCode.ToLower())
            }).Select(v => v).Distinct().OrderBy(t => t.text);

            var taskDataCampaign = tactic.Select(t => new
            {
                id = string.Format("V{0}_C{1}", t.VerticalId, t.Plan_Campaign_Program.PlanCampaignId),
                text = t.Plan_Campaign_Program.Plan_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.Plan_Campaign.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.Plan_Campaign_Program.Plan_Campaign.StartDate,
                                                          t.Plan_Campaign_Program.Plan_Campaign.EndDate),
                progress = 0,
                open = false,
                parent = string.Format("V{0}", t.VerticalId),
                color = Common.COLORC6EBF3_WITH_BORDER,
                plancampaignid = t.Plan_Campaign_Program.PlanCampaignId
            }).Select(t => t).Distinct().OrderBy(t => t.text);

            var taskDataProgram = tactic.Select(t => new
           {
               id = string.Format("V{0}_C{1}_P{2}", t.VerticalId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),
               text = t.Plan_Campaign_Program.Title,
               start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Plan_Campaign_Program.StartDate),
               duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.Plan_Campaign_Program.StartDate,
                                                          t.Plan_Campaign_Program.EndDate),
               progress = 0,
               open = false,
               parent = string.Format("V{0}_C{1}", t.VerticalId, t.Plan_Campaign_Program.PlanCampaignId),
               color = "",
               planprogramid = t.PlanProgramId
           }).Select(t => t).Distinct().OrderBy(t => t.text);

            var taskDataTactic = tactic.Select(t => new
            {
                id = string.Format("V{0}_C{1}_P{2}_T{3}", t.VerticalId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId),
                text = t.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.StartDate,
                                                          t.EndDate),
                progress = 0,
                open = false,
                parent = string.Format("V{0}_C{1}_P{2}", t.VerticalId, t.Plan_Campaign_Program.PlanCampaignId, t.PlanProgramId),
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.Vertical.ColorCode.ToLower()),
                plantacticid = t.PlanTacticId
            }).OrderBy(t => t.text);

            return taskDataVertical.Concat<object>(taskDataCampaign).Concat<object>(taskDataTactic).Concat<object>(taskDataProgram).ToList<object>();
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
        public List<object> GetTaskDetailTactic(List<Plan_Campaign_Program_Tactic> tactic)
        {
            string tacticStatusSubmitted = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            string tacticStatusDeclined = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;
            List<ProjectedRevenueClass> tacticList = ReportController.ProjectedRevenueCalculate((from t in tactic select t.PlanTacticId).ToList<int>());
            var taskDataTactic = tactic.Select(t => new
            {
                id = string.Format("C{0}_P{1}_T{2}_Y{3}", t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId, t.TacticTypeId),
                text = t.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.StartDate,
                                                          t.EndDate),
                progress = 0,
                open = false,
                parent = string.Format("C{0}_P{1}", t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId),
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.TacticType.ColorCode.ToLower()),
                isSubmitted = t.Status.Equals(tacticStatusSubmitted),
                isDeclined = t.Status.Equals(tacticStatusDeclined),
                inqs = t.INQs,
                mqls = t.MQLs,
                cost = t.Cost,
                cws = t.Status.Equals(tacticStatusSubmitted) || t.Status.Equals(tacticStatusDeclined) ? Math.Round(tacticList.Where(tl => tl.PlanTacticId == t.PlanTacticId).Select(tl => tl.ProjectedRevenue).SingleOrDefault(), 2) : 0,
                plantacticid = t.PlanTacticId
            }).OrderBy(t => t.text);

            var taskDataProgram = tactic.Select(p => new
            {
                id = string.Format("C{0}_P{1}", p.Plan_Campaign_Program.PlanCampaignId, p.PlanProgramId),
                text = p.Plan_Campaign_Program.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, p.Plan_Campaign_Program.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          p.Plan_Campaign_Program.StartDate,
                                                          p.Plan_Campaign_Program.EndDate),
                progress = 0,
                open = false,
                parent = string.Format("C{0}", p.Plan_Campaign_Program.PlanCampaignId),
                color = "",
                planprogramid = p.PlanProgramId
            }).Select(p => p).Distinct().OrderBy(p => p.text);

            var taskDataCampaign = tactic.Select(c => new
            {
                id = string.Format("C{0}", c.Plan_Campaign_Program.PlanCampaignId),
                text = c.Plan_Campaign_Program.Plan_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, c.Plan_Campaign_Program.Plan_Campaign.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          c.Plan_Campaign_Program.Plan_Campaign.StartDate,
                                                          c.Plan_Campaign_Program.Plan_Campaign.EndDate),
                progress = 0,
                open = false,
                color = Common.COLORC6EBF3_WITH_BORDER,
                plancampaignid = c.Plan_Campaign_Program.PlanCampaignId
            }).Select(c => c).Distinct().OrderBy(c => c.text);

            return taskDataCampaign.Concat<object>(taskDataTactic).Concat<object>(taskDataProgram).ToList<object>();
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
                                    Comment = string.Format("Improvement Tactic {0} by {1}", status, Sessions.User.DisplayName),
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
                                Comment = string.Format("Tactic {0} by {1}", status, Sessions.User.DisplayName),
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
                if (Convert.ToString(section) != "")
                {
                    if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        Plan_Campaign_Program_Tactic objPlan_Campaign_Program_Tactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(id)).SingleOrDefault();
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
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            InspectModel im = GetInspectModel(id, section);
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
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.Tactic).ToLower());
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
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.Tactic).ToLower());
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




            ViewBag.ReviewModel = (from tc in tacticComment
                                   where (tc.PlanTacticId.HasValue)
                                   select new InspectReviewModel
                                   {
                                       PlanTacticId = Convert.ToInt32(tc.PlanTacticId),
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
            ViewBag.TacticDetail = im;

            var businessunittitle = (from bun in db.BusinessUnits
                                     where bun.BusinessUnitId == im.BusinessUnitId
                                     select bun.Title).FirstOrDefault();
            ViewBag.BudinessUnitTitle = businessunittitle.ToString();
            bool isValidDirectorUser = false;
            bool isValidOwner = false;
            if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            {
               isValidDirectorUser = true;
            }
            if (im.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
            }
            ViewBag.IsValidDirectorUser = isValidDirectorUser;
            ViewBag.IsValidOwner = isValidOwner;
            return PartialView("Review");
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get InspectModel.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param section="id">.Decide which section to open for Inspect Popup (tactic,program or campaign)</param>
        /// <returns>Returns InspectModel.</returns>
        private InspectModel GetInspectModel(int id, string section)
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
                                  INQs = pcpt.INQs,
                                  MQLs = pcpt.MQLs,
                                  VerticalTitle = pcpt.Vertical.Title,
                                  AudiencTitle = pcpt.Audience.Title,
                                  INQsActual = pcpt.INQsActual == null ? 0 : pcpt.INQsActual,
                                  MQLsActual = pcpt.MQLsActual == null ? 0 : pcpt.MQLsActual,
                                  CostActual = pcpt.CostActual == null ? 0 : pcpt.CostActual,
                                  CWs = pcpt.CWs == null ? 0 : pcpt.CWs,
                                  CWsActual = pcpt.CWsActual == null ? 0 : pcpt.CWsActual,
                                  Revenues = pcpt.Revenues == null ? 0 : pcpt.Revenues,
                                  RevenuesActual = pcpt.RevenuesActual == null ? 0 : pcpt.RevenuesActual,
                                  ROI = pcpt.ROI == null ? 0 : pcpt.ROI,
                                  ROIActual = pcpt.ROIActual == null ? 0 : pcpt.ROIActual
                              }).SingleOrDefault();
                }
                if (section == Convert.ToString(Enums.Section.Program).ToLower())
                {
                    var objPlan_Campaign_Program = db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == id && pcp.IsDeleted == false).FirstOrDefault();


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
                    imodel.Cost = objPlan_Campaign_Program.Cost;
                    imodel.StartDate = objPlan_Campaign_Program.StartDate;
                    imodel.EndDate = objPlan_Campaign_Program.EndDate;
                    imodel.INQs = objPlan_Campaign_Program.INQs;
                    imodel.MQLs = objPlan_Campaign_Program.MQLs;
                    imodel.VerticalTitle = objPlan_Campaign_Program.Vertical.Title;
                    imodel.AudiencTitle = objPlan_Campaign_Program.Audience.Title;
                }

                if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                {

                    var objPlan_Campaign = db.Plan_Campaign.Where(pcp => pcp.PlanCampaignId == id && pcp.IsDeleted == false).FirstOrDefault();

                    int cntSumbitProgramStatus = db.Plan_Campaign_Program.Where(pcpt => pcpt.PlanCampaignId == id && pcpt.IsDeleted == false && !pcpt.Status.Equals(statussubmit)).Count();
                    int cntSumbitTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == id && pcpt.IsDeleted == false && !pcpt.Status.Equals(statussubmit)).Count();

                    int cntApproveProgramStatus = db.Plan_Campaign_Program.Where(pcpt => pcpt.PlanCampaignId == id && pcpt.IsDeleted == false && (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();
                    int cntApproveTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == id && pcpt.IsDeleted == false && (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();

                    int cntDeclineProgramStatus = db.Plan_Campaign_Program.Where(pcpt => pcpt.PlanCampaignId == id && pcpt.IsDeleted == false && !pcpt.Status.Equals(statusdecline)).Count();
                    int cntDeclineTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == id && pcpt.IsDeleted == false && !pcpt.Status.Equals(statusdecline)).Count();


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


                    imodel.CampaignTitle = objPlan_Campaign.Title;
                    imodel.Status = objPlan_Campaign.Status;
                    imodel.VerticalId = objPlan_Campaign.VerticalId;
                    imodel.ColorCode = Campaign_InspectPopup_Flag_Color;
                    imodel.Description = objPlan_Campaign.Description;
                    imodel.AudienceId = objPlan_Campaign.AudienceId;
                    imodel.PlanCampaignId = objPlan_Campaign.PlanCampaignId;
                    imodel.OwnerId = objPlan_Campaign.CreatedBy;
                    imodel.BusinessUnitId = objPlan_Campaign.Plan.Model.BusinessUnitId;
                    imodel.Cost = objPlan_Campaign.Cost;
                    imodel.StartDate = objPlan_Campaign.StartDate;
                    imodel.EndDate = objPlan_Campaign.EndDate;
                    imodel.INQs = objPlan_Campaign.INQs;
                    imodel.MQLs = objPlan_Campaign.MQLs;
                    imodel.VerticalTitle = objPlan_Campaign.Vertical.Title;
                    imodel.AudiencTitle = objPlan_Campaign.Audience.Title;

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
                                  VerticalId = pcpt.VerticalId,
                                  ColorCode = pcpt.ImprovementTacticType.ColorCode,
                                  Description = pcpt.Description,
                                  AudienceId = pcpt.AudienceId,
                                  PlanCampaignId = pcpt.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
                                  PlanProgramId = pcpt.ImprovementPlanProgramId,
                                  OwnerId = pcpt.CreatedBy,
                                  BusinessUnitId = pcpt.BusinessUnitId,
                                  Cost = pcpt.Cost,
                                  StartDate = pcpt.EffectiveDate,
                                  VerticalTitle = pcpt.Vertical.Title,
                                  AudiencTitle = pcpt.Audience.Title
                              }).SingleOrDefault();
                }

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return imodel;
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Actuals Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <returns>Returns Partial View Of Actuals Tab.</returns>
        public ActionResult LoadActuals(int id)
        {
            string[] aryStageTitle = new string[] { Enums.InspectStageValues[Enums.InspectStage.INQ.ToString()].ToString(), Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString(), Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString(), Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString() };
            ViewBag.StageTitle = aryStageTitle;
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.Tactic).ToLower());
            List<int> tid = new List<int>();
            tid.Add(id);
            List<ProjectedRevenueClass> tacticList = ReportController.ProjectedRevenueCalculate(tid);
            im.Revenues = Math.Round(tacticList.Where(tl => tl.PlanTacticId == id).Select(tl => tl.ProjectedRevenue).SingleOrDefault(), 2);
            tacticList = ReportController.ProjectedRevenueCalculate(tid, true);
            im.CWs = Math.Round(tacticList.Where(tl => tl.PlanTacticId == id).Select(tl => tl.ProjectedRevenue).SingleOrDefault(), 2);
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
            ViewBag.TacticDetail = im;
            bool isValidUser = true;
            if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            {
                if (im.OwnerId != Sessions.User.UserId) isValidUser = false;
            }
            ViewBag.IsValidUser = isValidUser;
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
        public JsonResult UploadResult(List<InspectActual> tacticactual)
        {
            try
            {
                if (tacticactual != null)
                {
                    var actualResult = (from t in tacticactual
                                        select new { t.PlanTacticId, t.TotalINQActual, t.TotalMQLActual, t.TotalCWActual, t.TotalRevenueActual, t.TotalCostActual, t.ROI, t.ROIActual, t.IsActual }).FirstOrDefault();
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            ObjectParameter ReturnValue = new ObjectParameter("ReturnValue", typeof(Int64));
                            db.Plan_Campaign_Program_Tactic_ActualDelete(actualResult.PlanTacticId, ReturnValue);
                            Int64 returnValue;
                            Int64 inq = 0, mql = 0, cw = 0;
                            double revenue = 0;
                            Int64.TryParse(ReturnValue.Value.ToString(), out returnValue);
                            if (returnValue == 0)
                            {
                                if (actualResult.IsActual)
                                {
                                foreach (var t in tacticactual)
                                {
                                    Plan_Campaign_Program_Tactic_Actual objpcpta = new Plan_Campaign_Program_Tactic_Actual();
                                    objpcpta.PlanTacticId = t.PlanTacticId;
                                    objpcpta.StageTitle = t.StageTitle;
                                    if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.INQ.ToString()].ToString()) inq += t.ActualValue;
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
                            objPCPT.INQsActual = inq;
                            objPCPT.MQLsActual = mql;
                            objPCPT.CWsActual = cw;
                            objPCPT.RevenuesActual = revenue;
                            objPCPT.CostActual = actualResult.TotalCostActual;
                            objPCPT.ROI = actualResult.ROI;
                            objPCPT.ROIActual = actualResult.ROIActual;
                            //objPCPT.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
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
                        tactic.VerticalId = form.VerticalId;
                    }

                    if (form.AudienceId != 0)
                    {
                        tactic.AudienceId = form.AudienceId;
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
        /// Action to Load User Image.
        /// </summary>
        /// <param name="id">User Id.</param>
        /// <returns>Returns View of Image.</returns>
        public ActionResult LoadUserImage(string id = null)
        {
            Guid userId = new Guid();
            byte[] imageBytes = Common.ReadFile(Server.MapPath("~") + "/content/images/user_image_not_found.png");
            try
            {
                if (id != null)
                {
                    userId = Guid.Parse(id);
                    User objUser = new User();

                    objUser = objBDSUserRepository.GetTeamMemberDetails(userId, Sessions.ApplicationId);
                    if (objUser != null)
                    {
                        if (objUser.ProfilePhoto != null)
                        {
                            imageBytes = objUser.ProfilePhoto;
                        }
                    }
                }
                if (imageBytes != null)
                {
                    MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                    image = Common.ImageResize(image, 35, 35, true, false);
                    imageBytes = Common.ImageToByteArray(image);
                    return File(imageBytes, "image/jpg");
                }
            }
            catch (Exception e)
            {
                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                    imageBytes = Common.ReadFile(Server.MapPath("~") + "/content/images/user_image_not_found.png");
                    MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                    image = Common.ImageResize(image, 35, 35, true, false);
                    imageBytes = Common.ImageToByteArray(image);
                    return File(imageBytes, "image/jpg");
                }
            }
            return View();
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
                                        }
                                    }
                                    if (result >= 1)
                                    {
                                        Common.mailSendForTactic(planTacticId, status, tactic.Title, false, "", Convert.ToString(Enums.Section.Tactic).ToLower());
                                    }
                                    strmessage = Common.objCached.TacticStatusSuccessfully.Replace("{0}", status);
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

                                        }
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
            InspectModel im = GetInspectModel(id, "program");
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
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.Program).ToLower());
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
            ViewBag.ProgramDetail = im;

            var businessunittitle = (from bun in db.BusinessUnits
                                     where bun.BusinessUnitId == im.BusinessUnitId
                                     select bun.Title).FirstOrDefault();
            ViewBag.BudinessUnitTitle = businessunittitle.ToString();
            bool isValidUser = false;
            if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            {
                if (im.OwnerId != Sessions.User.UserId) isValidUser = true;
            }
            ViewBag.IsValidUser = isValidUser;
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

            InspectModel im = GetInspectModel(id, "campaign");
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
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.Campaign).ToLower());
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
            ViewBag.CampaignDetail = im;

            var businessunittitle = (from bun in db.BusinessUnits
                                     where bun.BusinessUnitId == im.BusinessUnitId
                                     select bun.Title).FirstOrDefault();
            ViewBag.BudinessUnitTitle = businessunittitle.ToString();
            bool isValidUser = false;
            if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            {
                if (im.OwnerId != Sessions.User.UserId) isValidUser = true;
            }
            ViewBag.IsValidUser = isValidUser;
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
        public JsonResult GetNumberOfActivityPerMonthByPlanId(int planid, string strparam, string geographyIds, string individualsIds, bool isShowOnlyMyTactic = false)
        {
            string planYear = db.Plans.Single(p => p.PlanId.Equals(planid)).Year;
            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;
            Common.GetPlanGanttStartEndDate(planYear, strparam, ref CalendarStartDate, ref CalendarEndDate);

            //Start Maninder Singh Wadhva : 11/15/2013 - Getting list of tactic for view control for plan version id.
            //// Selecting campaign(s) of plan whose IsDelete=false.
            var campaign = db.Plan_Campaign.Where(pc => pc.PlanId.Equals(planid) && pc.IsDeleted.Equals(false))
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
            List<string> filterGeography = string.IsNullOrWhiteSpace(geographyIds) ? new List<string>() : geographyIds.Split(',').ToList();

            //// Individual filter criteria.
            List<string> filterIndividual = string.IsNullOrWhiteSpace(individualsIds) ? new List<string>() : individualsIds.Split(',').ToList();

            if (isShowOnlyMyTactic)
            {
                filterIndividual.Add(Sessions.User.UserId.ToString());
            }

            var objPlan_Campaign_Program_Tactic_Comment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.PlanTacticId != null).ToList()
                                                        .Where(pcptc => pcptc.Plan_Campaign_Program_Tactic.IsDeleted.Equals(false) &&
                                                            programId.Contains(pcptc.Plan_Campaign_Program_Tactic.PlanProgramId) &&
                                                            //// Individual & Show my Tactic Filter 
                                                           (filterIndividual.Count.Equals(0) || filterIndividual.Contains(pcptc.CreatedBy.ToString()))).ToList();

            //// Applying filters to tactic comment (IsDelete, Individuals and Show My Tactic)
            List<int?> tacticId = objPlan_Campaign_Program_Tactic_Comment
                                                        .Select(pcptc => pcptc.PlanTacticId).ToList();

            //// Applying filters to tactic (IsDelete, Geography, Individuals and Show My Tactic)
            var objPlan_Campaign_Program_Tactic = db.Plan_Campaign_Program_Tactic.ToList()
                                                        .Where(pcpt => pcpt.IsDeleted.Equals(false) &&
                                                                       programId.Contains(pcpt.PlanProgramId) &&
                                                            //// Checking start and end date
                                                        Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                    CalendarEndDate,
                                                                                                    pcpt.StartDate,
                                                                                                    pcpt.EndDate).Equals(false) &&
                                                            //// Geography Filter
                                                                       (filterGeography.Count.Equals(0) || filterGeography.Contains(pcpt.GeographyId.ToString())) &&
                                                            //// Individual & Show my Tactic Filter 
                                                                       (filterIndividual.Count.Equals(0) || filterIndividual.Contains(pcpt.CreatedBy.ToString()) || filterIndividual.Contains(pcpt.ModifiedBy.ToString()) || tacticId.Contains(pcpt.PlanTacticId)))
                                                        .Select(pcpt => pcpt);


            Dictionary<int, string> activitymonth = new Dictionary<int, string>();

            int[] montharray = new int[12];


            if (objPlan_Campaign_Program_Tactic != null && objPlan_Campaign_Program_Tactic.Count() > 0)
            {
                IEnumerable<string> diff;
                foreach (var a in objPlan_Campaign_Program_Tactic)
                {
                    var start = Convert.ToDateTime(a.StartDate);
                    var end = Convert.ToDateTime(a.EndDate);
                    int year = 0;
                    if (strparam != null)
                    {
                        if (strparam == Enums.UpcomingActivities.planYear.ToString() || strparam == "")
                        {
                            year = Convert.ToInt32(a.Plan_Campaign_Program.Plan_Campaign.Plan.Year);
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
            planmodel.objGeography = db.Geographies.Where(g => g.IsDeleted.Equals(false) && g.ClientId == Sessions.User.ClientId).Select(g => g).OrderBy(g => g.Title).ToList();
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();
            var individuals = bdsUserRepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, Sessions.IsSystemAdmin);
            planmodel.objIndividuals = individuals.OrderBy(i => string.Format("{0} {1}", i.FirstName, i.LastName)).ToList();
            List<TacticType> objTacticType = new List<TacticType>();

            //// Modified By: Maninder Singh for TFS Bug#282: Extra Tactics Displaying in Add Actual Screen
            objTacticType = (from t in db.Plan_Campaign_Program_Tactic
                             where t.Plan_Campaign_Program.Plan_Campaign.PlanId == Sessions.PlanId
                             && tacticStatus.Contains(t.Status) && t.IsDeleted == false
                             select t.TacticType).Distinct().ToList();

            ViewBag.TacticTypeList = objTacticType;
            return View(planmodel);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Actual Value of Tactic.
        /// </summary>
        /// <returns>Returns Json Result.</returns>
        public JsonResult GetActualTactic(int status)
        {
            List<int> tacticIds = new List<int>();
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            if (status == 0)
            {
                //// Modified By: Maninder Singh for TFS Bug#282: Extra Tactics Displaying in Add Actual Screen
                tacticIds = db.Plan_Campaign_Program_Tactic.Where(planTactic => planTactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(Sessions.PlanId) &&
                                                                       tacticStatus.Contains(planTactic.Status) &&
                                                                       planTactic.IsDeleted.Equals(false) && planTactic.CostActual == null &&
                                                                       !planTactic.Plan_Campaign_Program_Tactic_Actual.Any())
                                                            .Select(planTactic => planTactic.PlanTacticId).ToList();
            }
            else
            {
                tacticIds = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == Sessions.PlanId && tacticStatus.Contains(t.Status) && t.IsDeleted == false).Select(t => t.PlanTacticId).ToList();

            }

            var dtTactic = (from pt in db.Plan_Campaign_Program_Tactic_Actual
                            where tacticIds.Contains(pt.PlanTacticId)
                            select new { pt.PlanTacticId, pt.CreatedBy, pt.CreatedDate }).GroupBy(pt => pt.PlanTacticId).Select(pt => pt.FirstOrDefault());
            List<Guid> userListId = new List<Guid>();
            userListId = (from ta in dtTactic select ta.CreatedBy).ToList<Guid>();

            var tactic = db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId)).Select(t => t).ToList();
            List<ProjectedRevenueClass> tacticList = ReportController.ProjectedRevenueCalculate(tacticIds);
            List<ProjectedRevenueClass> tacticListCW = ReportController.ProjectedRevenueCalculate(tacticIds, true);
            var listModified = tactic.Where(t => t.ModifiedDate != null).Select(t => t).ToList();
            foreach (var t in listModified)
            {
                userListId.Add(new Guid(t.ModifiedBy.ToString()));
            }
            string userList = string.Join(",", userListId.Select(s => s.ToString()).ToArray());
            List<User> userName = objBDSUserRepository.GetMultipleTeamMemberDetails(userList, Sessions.ApplicationId);
            var tacticObj = tactic.Select(t => new
            {
                id = t.PlanTacticId,
                title = t.Title,
                inqProjected = t.INQs,
                inqActual = t.INQsActual == null ? 0 : t.INQsActual,
                mqlProjected = t.MQLs,
                mqlActual = t.MQLsActual == null ? 0 : t.MQLsActual,
                cwProjected = Math.Round(tacticListCW.Where(tl => tl.PlanTacticId == t.PlanTacticId).Select(tl => tl.ProjectedRevenue).SingleOrDefault(), 2),
                cwActual = t.CWsActual == null ? 0 : t.CWsActual,
                revenueProjected = Math.Round(tacticList.Where(tl => tl.PlanTacticId == t.PlanTacticId).Select(tl => tl.ProjectedRevenue).SingleOrDefault(), 2),
                revenueActual = t.RevenuesActual == null ? 0 : t.RevenuesActual,
                costProjected = t.Cost,
                costActual = t.CostActual == null ? 0 : t.CostActual,
                roiProjected = t.ROI == null ? 0 : t.ROI,
                roiActual = t.ROIActual == null ? 0 : t.ROIActual,
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
                }).Select(pcp => pcp).Distinct()
            }).Select(t => t).Distinct().OrderBy(t => t.id);
            return Json(tacticObj, JsonRequestBehavior.AllowGet);
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
                                    improvementTacticShare.EmailBody = string.Format("{0} {1}", notification.EmailContent, optionalMessage);
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
                                tacticShare.EmailBody = string.Format("{0} {1}", notification.EmailContent, optionalMessage);
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
            var individuals = bdsUserRepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, Sessions.IsSystemAdmin);
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

            ViewBag.ReviewModel = (from tc in tacticComment
                                   select new InspectReviewModel
                                   {
                                       PlanTacticId = Convert.ToInt32(tc.ImprovementPlanTacticId),
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
            ViewBag.TacticDetail = im;

            var businessunittitle = (from bun in db.BusinessUnits
                                     where bun.BusinessUnitId == im.BusinessUnitId
                                     select bun.Title).FirstOrDefault();
            ViewBag.BudinessUnitTitle = businessunittitle.ToString();
            bool isValidUser = false;
            if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            {
                if (im.OwnerId != Sessions.User.UserId) isValidUser = true;
            }
            ViewBag.IsValidUser = isValidUser;
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

            var tacticobj = ImprovementMetric.Select(p => new
            {
                MetricId = p.MetricId,
                MetricCode = p.MetricCode,
                MetricName = p.MetricName,
                MetricType = p.MetricType,
                BaseLineRate = p.BaseLineRate,
                PlanWithoutTactic = p.PlanWithoutTactic,
                PlanWithTactic = p.PlanWithTactic,
            }).Select(p => p).Distinct().OrderBy(p => p.MetricId);

            return Json(new { data = tacticobj }, JsonRequestBehavior.AllowGet);
        }
        #endregion
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

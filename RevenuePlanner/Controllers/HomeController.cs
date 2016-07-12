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
using System.Web;
using System.Xml;
using System.Data;
using Integration;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Reflection;
using System.Runtime.CompilerServices;

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
        Dictionary<Guid, User> lstUsers = new Dictionary<Guid, BDSService.User>();
        List<TacticType> TacticTypeList = new List<TacticType>();
        private BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
        CacheObject objCache = new CacheObject(); // Add By Nishant Sheth // Desc:: For get values from cache
        StoredProcedure objSp = new StoredProcedure();// Add By Nishant Sheth // Desc:: For get values with storedprocedure

        #endregion

        public HomeController()
        {
            if (System.Web.HttpContext.Current.Cache["CommonMsg"] == null)
            {
                Common.xmlMsgFilePath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.FullName + "\\" + System.Configuration.ConfigurationManager.AppSettings.Get("XMLCommonMsgFilePath");//Modify by Akashdeep Kadia on 09/05/2016 to resolve PL ticket #989.
                Common.objCached.loadMsg(Common.xmlMsgFilePath);
                System.Web.HttpContext.Current.Cache["CommonMsg"] = Common.objCached;
                CacheDependency dependency = new CacheDependency(Common.xmlMsgFilePath);
                System.Web.HttpContext.Current.Cache.Insert("CommonMsg", Common.objCached, dependency);
            }
            else
            {
                Common.objCached = (Message)System.Web.HttpContext.Current.Cache["CommonMsg"];

            }
        }

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
        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Home, int currentPlanId = 0, int planTacticId = 0, int planCampaignId = 0, int planProgramId = 0, bool isImprovement = false, bool isGridView = false, int planLineItemId = 0, bool IsPlanSelector = false, int PreviousPlanID = 0)
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
            //Added by Rahul Shah on 22/01/2016 for PL #1898
            ViewBag.IsPlanSelector = IsPlanSelector;

            ViewBag.PreviousPlanID = PreviousPlanID;


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
                if (activeMenu.Equals(Enums.ActiveMenu.Plan))
                {
                    latestPlan = activePlan.OrderBy(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).Select(plan => plan).FirstOrDefault();
                }
                else
                {
                    latestPlan = activePlan.Where(plan => plan.Status.Equals(planPublishedStatus)).OrderBy(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).Select(plan => plan).FirstOrDefault();
                }

                List<Plan> fiterActivePlan = new List<Plan>();
                fiterActivePlan = activePlan.Where(plan => Convert.ToInt32(plan.Year) < Convert.ToInt32(currentYear)).ToList();
                if (fiterActivePlan != null && fiterActivePlan.Any())
                {
                    if (activeMenu.Equals(Enums.ActiveMenu.Plan))
                    {
                        latestPlan = fiterActivePlan.OrderByDescending(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).FirstOrDefault();
                    }
                    else
                    {
                        latestPlan = fiterActivePlan.Where(plan => plan.Status.Equals(planPublishedStatus)).OrderByDescending(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).FirstOrDefault();

                    }
                    //  latestPlan = fiterActivePlan.OrderByDescending(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).FirstOrDefault();
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
            var Label = Enums.FilterLabel.Plan.ToString();

            //Added By komal Rawal for #1959 to handle last viewed data in session
            var FilterName = Sessions.FilterPresetName;
            var SetOFLastViews = new List<Plan_UserSavedViews>();
            if (Sessions.PlanUserSavedViews == null)
            {
                SetOFLastViews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            }
            else
            {
                //Modified by komal for internal issue
                if (FilterName != null && FilterName != "")
                {
                    SetOFLastViews = Sessions.PlanUserSavedViews.ToList();

                }
                else
                {
                    SetOFLastViews = Sessions.PlanUserSavedViews.Where(view => view.ViewName == null).ToList();
                }
            }
            //End
            var SetOfPlanSelected = SetOFLastViews.Where(view => view.FilterName == Label && view.Userid == Sessions.User.UserId).ToList();
            Common.PlanUserSavedViews = SetOFLastViews; // Add by Nishant Sheth #1915 // Change by nishant to handle null for plan tab menu
            if (Enums.ActiveMenu.Home.Equals(activeMenu))
            {
                //// Get list of Active(published) plans for all above models
                //Modified for #1750 by Komal Rawal
                var LastSetOfPlanSelected = new List<string>();
                var LastSetOfYearSelected = new List<string>();
                var Yearlabel = Enums.FilterLabel.Year.ToString();

                // var SetOFLastViews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();

                //    var SetOfPlanSelected = SetOFLastViews.Where(view => view.FilterName == Label && view.Userid == Sessions.User.UserId).ToList();
                var SetofLastYearsSelected = SetOFLastViews.Where(view => view.FilterName == Yearlabel && view.Userid == Sessions.User.UserId).ToList();
                var FinalSetOfPlanSelected = "";
                var FinalSetOfYearsSelected = "";
                if (FilterName != null && FilterName != "")
                {
                    FinalSetOfPlanSelected = SetOfPlanSelected.Where(view => view.ViewName == FilterName).Select(View => View.FilterValues).FirstOrDefault();
                    FinalSetOfYearsSelected = SetofLastYearsSelected.Where(view => view.ViewName == FilterName).Select(View => View.FilterValues).FirstOrDefault();
                }
                else
                {
                    FinalSetOfPlanSelected = SetOfPlanSelected.Where(view => view.IsDefaultPreset == true).Select(View => View.FilterValues).FirstOrDefault();
                    if (FinalSetOfPlanSelected == null)
                    {
                        FinalSetOfPlanSelected = SetOfPlanSelected.Where(view => view.ViewName == null).Select(View => View.FilterValues).FirstOrDefault();
                    }

                    FinalSetOfYearsSelected = SetofLastYearsSelected.Where(view => view.IsDefaultPreset == true).Select(View => View.FilterValues).FirstOrDefault();
                    if (FinalSetOfYearsSelected == null)
                    {
                        FinalSetOfYearsSelected = SetofLastYearsSelected.Where(view => view.ViewName == null).Select(View => View.FilterValues).FirstOrDefault();
                    }

                }
                if (FinalSetOfPlanSelected != null)
                {
                    LastSetOfPlanSelected = FinalSetOfPlanSelected.Split(',').ToList();
                }

                if (FinalSetOfYearsSelected != null)
                {
                    LastSetOfYearSelected = FinalSetOfYearsSelected.Split(',').ToList();
                }

                activePlan = activePlan.Where(plan => plan.Status.Equals(planPublishedStatus) && plan.IsDeleted == false).ToList();
                //Modified By komal Rawal for Year filter on home page.
                var SelectedYear = activePlan.Where(plan => plan.PlanId == currentPlan.PlanId).Select(plan => plan.Year).ToList();

                if (LastSetOfYearSelected.Count > 0)
                {
                    SelectedYear = activePlan.Where(plan => LastSetOfYearSelected.Contains(plan.Year.ToString())).Select(plan => plan.Year).Distinct().ToList();

                    if (SelectedYear.Count == 0)
                    {
                        SelectedYear = LastSetOfYearSelected;
                    }
                }
                else
                {
                    if (LastSetOfPlanSelected.Count > 0)
                    {
                        SelectedYear = activePlan.Where(plan => LastSetOfPlanSelected.Contains(plan.PlanId.ToString())).Select(plan => plan.Year).Distinct().ToList();
                    }
                }

                var uniqueplanids = activePlan.Select(p => p.PlanId).Distinct().ToList();

                var CampPlans = objDbMrpEntities.Plan_Campaign.Where(camp => camp.IsDeleted == false && uniqueplanids.Contains(camp.PlanId))
                    .Select(camp => new
                    {
                        PlanId = camp.PlanId,
                        StartYear = camp.StartDate.Year,
                        EndYear = camp.EndDate.Year,
                        StartDate = camp.StartDate,
                        EndDate = camp.EndDate
                    })
                    .ToList();

                var CampPlanIds = CampPlans.Where(camp => SelectedYear.Contains(camp.StartDate.Year.ToString()) || SelectedYear.Contains(camp.EndDate.Year.ToString()))
                    .Select(camp => camp.PlanId).Distinct().ToList();

                var PlanIds = activePlan.Where(plan => SelectedYear.Contains(plan.Year))
                 .Select(plan => plan.PlanId).Distinct().ToList();


                var allPlanIds = CampPlanIds.Concat(PlanIds).Distinct().ToList();

                // var TempIds = activePlan.Select(id => id.PlanId).ToList();
                var YearWiseListOfPlans = activePlan.Where(list => allPlanIds.Contains(list.PlanId)).ToList();

                planmodel.lstPlan = YearWiseListOfPlans.Select(plan => new PlanListModel
                {
                    PlanId = plan.PlanId,
                    Title = HttpUtility.HtmlDecode(plan.Title),
                    Checked = LastSetOfPlanSelected.Count.Equals(0) ? plan.PlanId == currentPlan.PlanId ? "checked" : "" : LastSetOfPlanSelected.Contains(plan.PlanId.ToString()) ? "checked" : "",
                    Year = plan.Year

                }).Where(plan => !string.IsNullOrEmpty(plan.Title)).OrderBy(plan => plan.Title, new AlphaNumericComparer()).ToList();

                List<SelectListItem> lstYear = new List<SelectListItem>();
                //  var SelectedYear = activePlan.Where(list => list.Checked == "checked").Select(list => list.Year).ToList();

                // List<Plan> tblPlan = db.Plans.Where(plan => plan.IsDeleted == false && plan.Status == published && plan.Model.ClientId == Sessions.User.ClientId).ToList();

                var StartYears = CampPlans.Select(camp => camp.StartYear)
             .Distinct().ToList();

                var EndYears = CampPlans.Select(camp => camp.EndYear)
                    .Distinct().ToList();

                var PlanYears = StartYears.Concat(EndYears).Distinct().ToList();

                var yearlist = PlanYears;// Modify BY Nishant Sheth #1821
                SelectListItem objYear = new SelectListItem();
                foreach (int years in yearlist)
                {
                    string yearname = Convert.ToString(years);
                    objYear = new SelectListItem();

                    objYear.Text = years.ToString();

                    objYear.Value = yearname;
                    objYear.Selected = SelectedYear.Contains(years.ToString()) ? true : false;
                    lstYear.Add(objYear);
                }
                //   ViewBag.SelectedYear = SelectedYear;

                ViewBag.ViewYear = lstYear.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();//@N Left Panel year list
                //End
                //Added By Maitri Gandhi for #2167
                if (LastSetOfPlanSelected.Count() == 1)
                {
                    if (LastSetOfPlanSelected.Contains(currentPlan.PlanId.ToString()) == false)
                    {
                        var LastViewedPlan = activePlan.Where(plan => LastSetOfPlanSelected.Contains(plan.PlanId.ToString())).Select(plan => plan).FirstOrDefault();
                        currentPlan = LastViewedPlan;

                    }
                }
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
                    //var SavedPlanIds = SetOfPlanSelected.Select(view => view.FilterValues).ToList();
                    //if(SavedPlanIds != null && SavedPlanIds.Count > 0)
                    //{
                    //    ViewBag.LastSavedPlanIDs = String.Join(",", SavedPlanIds);
                    //}
                    //else
                    //{

                    //    ViewBag.LastSavedPlanIDs = null;
                    //}



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
        public ActionResult HomePlan(string currentPlanId, string fltrYears) //modified By Komal as we pass comma separated string value in current plan id now
        {
            HomePlan objHomePlan = new HomePlan();

            //// Prepare ViewBy dropdown values
            List<ViewByModel> lstViewByTab = Common.GetDefaultGanttTypes(null);
            ViewBag.ViewByTab = lstViewByTab;

            //// Prepare upcoming activity dropdown values
            List<SelectListItem> lstUpComingActivity = UpComingActivity(Convert.ToString(currentPlanId), fltrYears);
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
            //TacticTypeList = objDbMrpEntities.TacticTypes.Where(tt => tt.IsDeleted == false).Select(tt => tt).ToList();
            objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId).ForEach(u => lstUsers.Add(u.UserId, u));
            //End
            //// Create plan list based on PlanIds of search filter

            List<int> planIds = string.IsNullOrWhiteSpace(planId) ? new List<int>() : planId.Split(',').Select(plan => int.Parse(plan)).ToList();
            // Modified by Nishant Sheth #1915
            //List<Plan> lstPlans1 = objDbMrpEntities.Plans.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && plan.Model.ClientId.Equals(Sessions.User.ClientId)).Select(plan => plan).ToList();

            DataSet dsPlanCampProgTac = new DataSet();
            dsPlanCampProgTac = (DataSet)objCache.Returncache(Enums.CacheObject.dsPlanCampProgTac.ToString());
            //List<Plan> lstPlans = dsPlanCampProgTac.Tables[0].AsEnumerable().Select(row => new Plan
            //{
            //    AllocatedBy = Convert.ToString(row["AllocatedBy"]),
            //    Budget = Convert.ToDouble(Convert.ToString(row["Budget"])),
            //    CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
            //    CreatedDate = Convert.ToDateTime(Convert.ToString(row["CreatedDate"])),
            //    DependencyDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["DependencyDate"])) ? (DateTime?)null : row["DependencyDate"]),
            //    Description = (Convert.ToString(row["Description"])),
            //    EloquaFolderPath = (Convert.ToString(row["EloquaFolderPath"])),
            //    GoalType = (Convert.ToString(row["GoalType"])),
            //    GoalValue = Convert.ToDouble(Convert.ToString(row["GoalValue"])),
            //    IsActive = Convert.ToBoolean(Convert.ToString(row["IsActive"])),
            //    IsDeleted = Convert.ToBoolean(Convert.ToString(row["IsDeleted"])),
            //    ModelId = int.Parse(Convert.ToString(row["ModelId"])),
            //    ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
            //    ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
            //    PlanId = int.Parse(Convert.ToString(row["PlanId"])),
            //    Status = Convert.ToString(row["Status"]),
            //    Title = Convert.ToString(row["Title"]),
            //    Version = Convert.ToString(row["Version"]),
            //    Year = Convert.ToString(row["Year"])
            //}).ToList();
            List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
            // Add By Nishant Sheth // Desc:: For get values from cache
            objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);
            // Add By Nishant Sheth
            // DESC :: To get selected plans tactictype from plan model ticket #1798
            List<int> ModelId = lstPlans.Select(plan => plan.ModelId).ToList();
            //Added By Komal Rawal to get all the user names for honeycomb feature
            // Modified By Nishant Sheth 
            // Desc :: #1798 Performance 
            TacticTypeList = objDbMrpEntities.TacticTypes.Where(tt => ModelId.Contains(tt.ModelId) && tt.IsDeleted == false).Select(tt => tt).ToList();
            bool IsRequest = false;
            bool IsFiltered = false;
            string planYear = string.Empty;
            int year;
            bool isNumeric = int.TryParse(timeFrame, out year);
            string[] listYear = timeFrame.Split('-');
            //bool isNumeric = int.TryParse(listYear[0], out year);

            if (isNumeric)
            {
                planYear = Convert.ToString(year);
            }
            else
            {
                // Add By Nishant Sheth
                // Desc :: To Resolved gantt chart year issue
                if (int.TryParse(listYear[0], out year))
                {
                    planYear = Convert.ToString(listYear[0]);
                }
                else
                {
                    planYear = DateTime.Now.Year.ToString();
                }
                // End By Nishant Sheth
            }



            // Modified by Nishant Sheth #1915

            // Modified By Nishant Sheth Date:30-Jan-2016
            // Desc :: To resolve the display all plan if there is no tactic 
            //List<Plan> lstPlans = objDbMrpEntities.Plans.Where(plan =>
            //    (!isNumeric ? (plan.Year == planYear) : (listYear.Contains(plan.Year))) &&
            //    planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && plan.Model.ClientId.Equals(Sessions.User.ClientId)).Select(plan => plan).ToList();
            // End By Nishant Sheth

            // Add By Nishant Sheth
            // DESC:: For get default filter view after user log out #1750

            var Label = Enums.FilterLabel.Plan.ToString();
            //var SetOfPlanSelected = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.FilterName != Label && view.Userid == Sessions.User.UserId && view.ViewName == null).Select(View => View).ToList();
            var SetOfPlanSelected = Common.PlanUserSavedViews;// Add by Nishant Sheth #1915
            // Add By Nishant Sheth
            // Desc :: To resolve the select and deselct all owner issues
            //  string planselectedowner = SetOfPlanSelected.Where(view => view.FilterName == Enums.FilterLabel.Owner.ToString()).Select(view => view.FilterValues).FirstOrDefault();
            // End By Nishant Sheth
            bool issetofplan = false;
            if (SetOfPlanSelected.Where(view => (view.FilterName != Enums.FilterLabel.Owner.ToString() && view.FilterName != Enums.FilterLabel.Status.ToString())).ToList().Count > 0)
            {
                issetofplan = true;
            }
            // Check filter flag other than owner & status
            if ((customFieldIds != "" || TacticTypeid != "") && issetofplan)
            {
                IsFiltered = true;

            }

            //// Get list of planIds from filtered plans

            var filteredPlanIds = lstPlans.Where(plan => plan.Year == planYear || planId.Split(',').Contains(plan.PlanId.ToString())).ToList().Select(plan => plan.PlanId).ToList();


            CalendarStartDate = CalendarEndDate = DateTime.Now;
            Common.GetPlanGanttStartEndDate(planYear, timeFrame, ref CalendarStartDate, ref CalendarEndDate);

            //// Start Maninder Singh Wadhva : 11/15/2013 - Getting list of tactic for view control for plan version id.
            //// Selecting campaign(s) of plan whose IsDelete=false.

            //var lstCampaign = objDbMrpEntities.Plan_Campaign.Where(campaign => filteredPlanIds.Contains(campaign.PlanId) && campaign.IsDeleted.Equals(false) && (!((campaign.EndDate < CalendarStartDate) || (campaign.StartDate > CalendarEndDate))))
            //                               .Select(campaign => campaign).ToList();
            //var lstCampaign = dsPlanCampProgTac.Tables[1].AsEnumerable().Select(row => new Plan_Campaign
            //{
            //    Abbreviation = Convert.ToString(row["Abbreviation"]),
            //    CampaignBudget = Convert.ToDouble(row["CampaignBudget"]),
            //    CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
            //    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
            //    Description = Convert.ToString(row["Description"]),
            //    EndDate = Convert.ToDateTime(row["EndDate"]),
            //    IntegrationInstanceCampaignId = Convert.ToString(row["IntegrationInstanceCampaignId"]),
            //    IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
            //    IsDeployedToIntegration = Convert.ToBoolean(row["IsDeployedToIntegration"]),
            //    LastSyncDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["LastSyncDate"])) ? (DateTime?)null : row["LastSyncDate"]),
            //    ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
            //    ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
            //    PlanCampaignId = Convert.ToInt32(row["PlanCampaignId"]),
            //    PlanId = Convert.ToInt32(row["PlanId"]),
            //    StartDate = Convert.ToDateTime(row["StartDate"]),
            //    Status = Convert.ToString(row["Status"]),
            //    Title = Convert.ToString(row["Title"])
            //}).ToList().Where(campaign => (!((campaign.EndDate < CalendarStartDate) || (campaign.StartDate > CalendarEndDate)))).ToList();

            var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).Where(campaign => (!((campaign.EndDate < CalendarStartDate) || (campaign.StartDate > CalendarEndDate)))).ToList();
            //List<Plan_Campaign> lstCampaign = ((List<Plan_Campaign>)objCache.Returncache(Enums.CacheObject.Campaign.ToString())).ToList();
            // Add By Nishant Sheth // Desc:: For get values from cache
            objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

            //// Selecting campaignIds.
            List<int> lstCampaignId = lstCampaign.Select(campaign => campaign.PlanCampaignId).ToList();

            //// Selecting program(s) of campaignIds whose IsDelete=false.
            //var lstProgram = objDbMrpEntities.Plan_Campaign_Program.Where(program => lstCampaignId.Contains(program.PlanCampaignId) && program.IsDeleted.Equals(false) && (!((program.EndDate < CalendarStartDate) || (program.StartDate > CalendarEndDate))))
            //                                      .Select(program => program)
            //                                      .ToList();
            List<Plan_Campaign_Program> lstProgram = new List<Plan_Campaign_Program>();
            List<Plan_Tactic> lstTactic = new List<Plan_Tactic>();
            List<Custom_Plan_Campaign_Program_Tactic> lstTacticPer = new List<Custom_Plan_Campaign_Program_Tactic>();
            List<Custom_Plan_Campaign_Program> lstProgramPer = new List<Custom_Plan_Campaign_Program>();
            List<int> lstProgramId = new List<int>();
            //// Owner filter criteria.
            List<Guid> filterOwner = string.IsNullOrWhiteSpace(ownerIds) ? new List<Guid>() : ownerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();
            //TacticType filter criteria
            List<int> filterTacticType = string.IsNullOrWhiteSpace(TacticTypeid) ? new List<int>() : TacticTypeid.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();
            //TacticType filter criteria
            List<string> filterStatus = string.IsNullOrWhiteSpace(StatusIds) ? new List<string>() : StatusIds.Split(',').Select(tactictype => tactictype).ToList();
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            int checklistyear = 0;
            List<string> lstFilteredCustomFieldOptionIds = new List<string>();
            List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
            string[] filteredCustomFields = string.IsNullOrWhiteSpace(customFieldIds) ? null : customFieldIds.Split(',');
            List<int> lstTacticIds;
            string requestCount = "";
            object improvementTacticForAccordion = new object();
            object improvementTacticTypeForAccordion = new object();
            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, activeMenu.ToLower());
            List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic = objDbMrpEntities.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => filteredPlanIds.Contains(improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId) &&
                                                                                      improvementTactic.IsDeleted.Equals(false) &&
                                                                                      (improvementTactic.EffectiveDate > CalendarEndDate).Equals(false))
                                                                               .Select(improvementTactic => improvementTactic).ToList();
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

            if (viewBy.Equals(PlanGanttTypes.Tactic.ToString(), StringComparison.OrdinalIgnoreCase) || viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]).Where(prog => (!((prog.EndDate < CalendarStartDate) || (prog.StartDate > CalendarEndDate)))).ToList();
                //var lstProgram = ((List<Plan_Campaign_Program>)objCache.Returncache(Enums.CacheObject.Program.ToString())).ToList();
                // Add By Nishant Sheth // Desc:: For get values from cache
                objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);
                lstProgramId = lstProgramPer.Select(program => program.PlanProgramId).ToList();
                //Modified By Komal Rawal for #2059 display no tactics if filters are deselected
                lstTacticPer = ((List<Custom_Plan_Campaign_Program_Tactic>)objCache.Returncache(Enums.CacheObject.CustomTactic.ToString()))
                                                                       .Where(tactic => tactic.IsDeleted.Equals(false) &&
                                                                       lstProgramId.Contains(tactic.PlanProgramId) &&
                                                                       (filterOwner.Contains(tactic.CreatedBy)) &&
                                                                       (filterTacticType.Contains(tactic.TacticTypeId)) &&
                                                                       (filterStatus.Contains(tactic.Status)) &&
                                                                       (!((tactic.EndDate < CalendarStartDate) || (tactic.StartDate > CalendarEndDate)))).ToList();
                objCache.AddCache(Enums.CacheObject.Tactic.ToString(), lstTacticPer.ToList());
                if (int.TryParse(listYear[0], out checklistyear))
                {
                    lstTacticPer = lstTacticPer.Where(tactic => listYear.Contains(tactic.StartDate.Year.ToString())
                        || listYear.Contains(tactic.EndDate.Year.ToString())).ToList();
                }
                if (lstTacticPer.Count() > 0)
                {
                    lstTacticIds = lstTacticPer.Select(tactic => tactic.PlanTacticId).ToList();
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
                        lstTacticPer = lstTacticPer.Where(tactic => lstTacticIds.Contains(tactic.PlanTacticId)).ToList();
                    }
                    List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);
                    lstTacticPer = lstTacticPer.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || (filterOwner.Contains(tactic.CreatedBy))).Select(tactic => tactic).ToList();
                }

                //// Get list of tactic and improvementTactic for which selected users are Owner
                var subordinatesTactic = lstTacticPer.Where(tactic => lstSubordinatesWithPeers.Contains(tactic.CreatedBy)).ToList();
                var subordinatesImprovementTactic = lstImprovementTactic.Where(improvementTactic => lstSubordinatesWithPeers.Contains(improvementTactic.CreatedBy)).ToList();

                //// Get Submitted tactic count for Request tab
                string tacticStatus = Enums.TacticStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
                //// Modified By Maninder Singh Wadhva PL Ticket#47, Modofied by Dharmraj #538
                requestCount = Convert.ToString(subordinatesTactic.Where(tactic => tactic.Status.Equals(tacticStatus)).Count() + subordinatesImprovementTactic.Where(improvementTactic => improvementTactic.Status.Equals(tacticStatus)).Count());



                var tacticForAllTabs = lstTacticPer.ToList();

                //// Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
                if (viewBy.Equals(PlanGanttTypes.Request.ToString()))
                {
                    lstTacticPer = subordinatesTactic;
                    lstImprovementTactic = subordinatesImprovementTactic;
                }

                if (objactivemenu.Equals(Enums.ActiveMenu.Plan))
                {

                    if (viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        IsRequest = true;
                        List<string> status = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                        List<string> statusCD = new List<string>();
                        statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                        statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

                        lstTacticPer = lstTacticPer.Where(t => status.Contains(t.Status) || ((t.CreatedBy == Sessions.User.UserId && !viewBy.Equals(PlanGanttTypes.Request.ToString())) ? statusCD.Contains(t.Status) : false))
                                        .Select(planTactic => planTactic).ToList();

                        //List<string> improvementTacticStatus = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                        lstImprovementTactic = lstImprovementTactic.Where(improvementTactic => status.Contains(improvementTactic.Status) || ((improvementTactic.CreatedBy == Sessions.User.UserId && !viewBy.Equals(PlanGanttTypes.Request.ToString())) ? statusCD.Contains(improvementTactic.Status) : false))
                                                                   .Select(improvementTactic => improvementTactic).ToList();

                        List<int> reqPlanIds = lstTacticPer.Select(a => a.PlanId).Distinct().ToList();
                        List<int> reqCampIds = lstTacticPer.Select(a => a.PlanCampaignId).Distinct().ToList();
                        List<int> reqProgIds = lstTacticPer.Select(a => a.PlanProgramId).Distinct().ToList();
                        lstPlans = lstPlans.Where(a => reqPlanIds.Contains(a.PlanId)).ToList();
                        lstCampaign = lstCampaign.Where(a => reqCampIds.Contains(a.PlanCampaignId)).ToList();
                        lstProgramPer = lstProgramPer.Where(a => reqProgIds.Contains(a.PlanProgramId)).ToList();



                    }
                    else
                    {

                        lstTacticPer = lstTacticPer.Where(t => !viewBy.Equals(PlanGanttTypes.Request.ToString()))
                                        .Select(planTactic => planTactic).ToList();

                        if (IsFiltered && lstTacticPer.Count() == 0)
                        {
                            lstImprovementTactic = new List<Plan_Improvement_Campaign_Program_Tactic>();
                        }
                        else
                        {
                            lstImprovementTactic = lstImprovementTactic.Where(improvementTactic => improvementTactic.IsDeleted == false && !viewBy.Equals(PlanGanttTypes.Request.ToString()))
                                                                       .Select(improvementTactic => improvementTactic).ToList();

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
                    lstTacticPer = lstTacticPer.Where(t => status.Contains(t.Status) || (!viewBy.Equals(PlanGanttTypes.Request.ToString()) ? statusCD.Contains(t.Status) : false)).Select(planTactic => planTactic).ToList();
                    if (IsRequest)
                    {
                        List<int> reqPlanIds = lstTacticPer.Select(a => a.PlanId).Distinct().ToList();
                        List<int> reqCampIds = lstTacticPer.Select(a => a.PlanCampaignId).Distinct().ToList();
                        List<int> reqProgIds = lstTacticPer.Select(a => a.PlanProgramId).Distinct().ToList();
                        lstPlans = lstPlans.Where(a => reqPlanIds.Contains(a.PlanId)).ToList();
                        lstCampaign = lstCampaign.Where(a => reqCampIds.Contains(a.PlanCampaignId)).ToList();
                        lstProgramPer = lstProgramPer.Where(a => reqProgIds.Contains(a.PlanProgramId)).ToList();
                    }
                    if (IsFiltered && lstTacticPer.Count() == 0)
                    {
                        lstImprovementTactic = new List<Plan_Improvement_Campaign_Program_Tactic>();
                    }
                    else
                    {
                        lstImprovementTactic = lstImprovementTactic.Where(improveTactic => status.Contains(improveTactic.Status) || (!viewBy.Equals(PlanGanttTypes.Request.ToString()) ? statusCD.Contains(improveTactic.Status) : false))
                                                                   .Select(improveTactic => improveTactic).ToList();

                    }
                    //// Prepare objects for ImprovementTactic section of Left side accordian pane
                    improvementTacticForAccordion = GetImprovementTacticForAccordion(lstImprovementTactic);
                    improvementTacticTypeForAccordion = GetImprovementTacticTypeForAccordion(lstImprovementTactic);
                }
            }
            else
            {
                //lstProgram = objDbMrpEntities.Plan_Campaign_Program.Where(program => lstCampaignId.Contains(program.PlanCampaignId) && program.IsDeleted.Equals(false) && (!((program.EndDate < CalendarStartDate) || (program.StartDate > CalendarEndDate))))
                //                                  .Select(program => program)
                //                                  .ToList();
                lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]).Where(prog => (!((prog.EndDate < CalendarStartDate) || (prog.StartDate > CalendarEndDate)))).ToList();
                objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);
                lstProgramId = lstProgramPer.Select(program => program.PlanProgramId).ToList();
                //lstTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
                //                                                           lstProgramId.Contains(tactic.PlanProgramId) &&
                //                                                           (filterOwner.Count.Equals(0) || filterOwner.Contains(tactic.CreatedBy)) &&
                //                                                            (filterTacticType.Count.Equals(0) || filterTacticType.Contains(tactic.TacticType.TacticTypeId)) &&
                //                                                            (filterStatus.Count.Equals(0) || filterStatus.Contains(tactic.Status)))
                //                                                           .ToList().Select(tactic => new Plan_Tactic
                //                                                           {
                //                                                               objPlanTactic = tactic,
                //                                                               PlanCampaignId = tactic.Plan_Campaign_Program.PlanCampaignId,
                //                                                               PlanId = tactic.Plan_Campaign_Program.Plan_Campaign.PlanId,
                //                                                               objPlanTacticProgram = tactic.Plan_Campaign_Program,
                //                                                               objPlanTacticCampaign = tactic.Plan_Campaign_Program.Plan_Campaign,
                //                                                               objPlanTacticCampaignPlan = tactic.Plan_Campaign_Program.Plan_Campaign.Plan,
                //                                                               TacticType = tactic.TacticType,
                //                                                               CreatedBy = tactic.CreatedBy,
                //                                                               StartDate = tactic.StartDate,
                //                                                               EndDate = tactic.EndDate
                //                                                           }).ToList();

                lstTacticPer = ((List<Custom_Plan_Campaign_Program_Tactic>)objCache.Returncache(Enums.CacheObject.CustomTactic.ToString()))
                                                                       .Where(tactic => tactic.IsDeleted.Equals(false) &&
                                                                       lstProgramId.Contains(tactic.PlanProgramId) &&
                                                                       (filterOwner.Contains(tactic.CreatedBy)) &&
                                                                       (filterTacticType.Contains(tactic.TacticTypeId)) &&
                                                                       (filterStatus.Contains(tactic.Status)) &&
                                                                       (!((tactic.EndDate < CalendarStartDate) || (tactic.StartDate > CalendarEndDate)))).ToList();

                objCache.AddCache(Enums.CacheObject.Tactic.ToString(), lstTacticPer.ToList());

                if (int.TryParse(listYear[0], out checklistyear))
                {
                    lstTacticPer = lstTacticPer.Where(tactic => listYear.Contains(tactic.StartDate.Year.ToString())
                        || listYear.Contains(tactic.EndDate.Year.ToString())).ToList();
                }
                if (lstTacticPer.Count() > 0)
                {
                    lstTacticIds = lstTacticPer.Select(tactic => tactic.PlanTacticId).ToList();
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
                        lstTacticPer = lstTacticPer.Where(tactic => lstTacticIds.Contains(tactic.PlanTacticId)).ToList();
                    }
                    List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);
                    lstTacticPer = lstTacticPer.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || (filterOwner.Contains(tactic.CreatedBy))).Select(tactic => tactic).ToList();
                }

                //// Get list of tactic and improvementTactic for which selected users are Owner
                var subordinatesTactic = lstTacticPer.Where(tactic => lstSubordinatesWithPeers.Contains(tactic.CreatedBy)).ToList();
                var subordinatesImprovementTactic = lstImprovementTactic.Where(improvementTactic => lstSubordinatesWithPeers.Contains(improvementTactic.CreatedBy)).ToList();

                //// Get Submitted tactic count for Request tab
                string tacticStatus = Enums.TacticStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
                //// Modified By Maninder Singh Wadhva PL Ticket#47, Modofied by Dharmraj #538
                requestCount = Convert.ToString(subordinatesTactic.Where(tactic => tactic.Status.Equals(tacticStatus)).Count() + subordinatesImprovementTactic.Where(improvementTactic => improvementTactic.Status.Equals(tacticStatus)).Count());

                var tacticForAllTabs = lstTacticPer.ToList();

                //// Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
                if (viewBy.Equals(PlanGanttTypes.Request.ToString()))
                {
                    lstTacticPer = subordinatesTactic;
                    lstImprovementTactic = subordinatesImprovementTactic;
                }

                if (objactivemenu.Equals(Enums.ActiveMenu.Plan))
                {

                    if (viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        IsRequest = true;
                        List<string> status = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                        List<string> statusCD = new List<string>();
                        statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                        statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

                        lstTacticPer = lstTacticPer.Where(t => status.Contains(t.Status) || ((t.CreatedBy == Sessions.User.UserId && !viewBy.Equals(PlanGanttTypes.Request.ToString())) ? statusCD.Contains(t.Status) : false))
                                     .Select(planTactic => planTactic).ToList();

                        //List<string> improvementTacticStatus = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                        lstImprovementTactic = lstImprovementTactic.Where(improvementTactic => status.Contains(improvementTactic.Status) || ((improvementTactic.CreatedBy == Sessions.User.UserId && !viewBy.Equals(PlanGanttTypes.Request.ToString())) ? statusCD.Contains(improvementTactic.Status) : false))
                                                                   .Select(improvementTactic => improvementTactic).ToList();


                    }
                    else
                    {

                        lstTacticPer = lstTacticPer.Where(t => t.IsDeleted == false && !viewBy.Equals(PlanGanttTypes.Request.ToString()))
                                     .Select(planTactic => planTactic).ToList();

                        if (IsFiltered && lstTacticPer.Count() == 0)
                        {
                            lstImprovementTactic = new List<Plan_Improvement_Campaign_Program_Tactic>();
                        }
                        else
                        {
                            lstImprovementTactic = lstImprovementTactic.Where(improvementTactic => improvementTactic.IsDeleted == false && !viewBy.Equals(PlanGanttTypes.Request.ToString()))
                                                                       .Select(improvementTactic => improvementTactic).ToList();

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
                    lstTacticPer = lstTacticPer.Where(t => status.Contains(t.Status) || (!viewBy.Equals(PlanGanttTypes.Request.ToString()) ? statusCD.Contains(t.Status) : false)).Select(planTactic => planTactic).ToList();

                    if (IsFiltered && lstTacticPer.Count() == 0)
                    {
                        lstImprovementTactic = new List<Plan_Improvement_Campaign_Program_Tactic>();
                    }
                    else
                    {
                        lstImprovementTactic = lstImprovementTactic.Where(improveTactic => status.Contains(improveTactic.Status) || (!viewBy.Equals(PlanGanttTypes.Request.ToString()) ? statusCD.Contains(improveTactic.Status) : false))
                                                                   .Select(improveTactic => improveTactic).ToList();

                    }
                    //// Prepare objects for ImprovementTactic section of Left side accordian pane
                    improvementTacticForAccordion = GetImprovementTacticForAccordion(lstImprovementTactic);
                    improvementTacticTypeForAccordion = GetImprovementTacticTypeForAccordion(lstImprovementTactic);
                }
            }

            #region Old Code
            //var lstProgramPer = dsPlanCampProgTac.Tables[2].AsEnumerable().Select(row => new Custom_Plan_Campaign_Program
            //{
            //    Abbreviation = Convert.ToString(row["Abbreviation"]),
            //    CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
            //    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
            //    Description = Convert.ToString(row["Description"]),
            //    EndDate = Convert.ToDateTime(row["EndDate"]),
            //    IntegrationInstanceProgramId = Convert.ToString(row["IntegrationInstanceProgramId"]),
            //    IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
            //    IsDeployedToIntegration = Convert.ToBoolean(row["IsDeployedToIntegration"]),
            //    LastSyncDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["LastSyncDate"])) ? (DateTime?)null : row["LastSyncDate"]),
            //    ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
            //    ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
            //    PlanCampaignId = Convert.ToInt32(row["PlanCampaignId"]),
            //    PlanProgramId = Convert.ToInt32(row["PlanProgramId"]),
            //    ProgramBudget = Convert.ToDouble(row["ProgramBudget"]),
            //    StartDate = Convert.ToDateTime(row["StartDate"]),
            //    Status = Convert.ToString(row["Status"]),
            //    Title = Convert.ToString(row["Title"]),
            //    PlanId = Convert.ToInt32(row["PlanId"])
            //}).ToList();

            //// Selecting programIds.


            //// Owner filter criteria.
            // List<Guid> filterOwner = string.IsNullOrWhiteSpace(ownerIds) ? new List<Guid>() : ownerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();
            // Add By Nishant Sheth
            // Desc :: To resolve the select and deselct all owner issues
            //if (planselectedowner == null)
            //{
            //    filterOwner = Sessions.User.UserId.ToString().Split(',').Select(owner => Guid.Parse(owner)).ToList();
            //}
            // End By Nishant Sheth
            //List<Guid> filterOwner = string.IsNullOrWhiteSpace(ownerIds) ? Sessions.User.UserId.ToString().Split(',').Select(owner => Guid.Parse(owner)).ToList() : ownerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();

            //Modified by komal rawal for #1283
            //TacticType filter criteria
            //List<int> filterTacticType = string.IsNullOrWhiteSpace(TacticTypeid) ? new List<int>() : TacticTypeid.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();
            //TacticType filter criteria
            //List<string> filterStatus = string.IsNullOrWhiteSpace(StatusIds) ? new List<string>() : StatusIds.Split(',').Select(tactictype => tactictype).ToList();
            //bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);

            //// Applying filters to tactic (IsDelete, Individuals)

            //List<Plan_Tactic> lstTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
            //                                                           lstProgramId.Contains(tactic.PlanProgramId) &&
            //                                                           (filterOwner.Count.Equals(0) || filterOwner.Contains(tactic.CreatedBy)) &&
            //                                                            (filterTacticType.Count.Equals(0) || filterTacticType.Contains(tactic.TacticType.TacticTypeId)) &&
            //                                                            (filterStatus.Count.Equals(0) || filterStatus.Contains(tactic.Status)))
            //                                                           .ToList().Select(tactic => new Plan_Tactic
            //                                                           {
            //                                                               objPlanTactic = tactic,
            //                                                               PlanCampaignId = tactic.Plan_Campaign_Program.PlanCampaignId,
            //                                                               PlanId = tactic.Plan_Campaign_Program.Plan_Campaign.PlanId,
            //                                                               objPlanTacticProgram = tactic.Plan_Campaign_Program,
            //                                                               objPlanTacticCampaign = tactic.Plan_Campaign_Program.Plan_Campaign,
            //                                                               objPlanTacticCampaignPlan = tactic.Plan_Campaign_Program.Plan_Campaign.Plan,
            //                                                               TacticType = tactic.TacticType,
            //                                                               CreatedBy = tactic.CreatedBy,
            //                                                               StartDate = tactic.StartDate,
            //                                                               EndDate = tactic.EndDate
            //                                                           }).ToList();


            // Add By Nishant Sheth // Desc:: For get values from cache

            // Add By Nishant Sheth
            // Desc :: To resolved the this month and this quarter issue
            //int checklistyear = 0;
            //if (int.TryParse(listYear[0], out checklistyear))
            //{
            //    lstTacticPer = lstTacticPer.Where(tactic => listYear.Contains(tactic.StartDate.Year.ToString())
            //        || listYear.Contains(tactic.EndDate.Year.ToString())).ToList();
            //}
            // End By Nishant Sheth

            //List<string> lstFilteredCustomFieldOptionIds = new List<string>();
            //List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
            //// Apply Custom restriction for None type
            //if (lstTacticPer.Count() > 0)
            //{
            //    List<int> lstTacticIds = lstTacticPer.Select(tactic => tactic.PlanTacticId).ToList();

            //    //// Start - Added by Sohel Pathan on 16/01/2015 for PL ticket #1134
            //    //// Custom Field Filter Criteria.
            //    //  List<string> filteredCustomFields = string.IsNullOrWhiteSpace(customFieldIds) ? new List<string>() : customFieldIds.Split(',').Select(customFieldId => customFieldId.ToString()).ToList();
            //    string[] filteredCustomFields = string.IsNullOrWhiteSpace(customFieldIds) ? null : customFieldIds.Split(',');
            //    if (filteredCustomFields != null)
            //    {
            //        //  filteredCustomFields.ForEach(customField =>
            //        foreach (string customField in filteredCustomFields)
            //        {
            //            string[] splittedCustomField = customField.Split('_');
            //            lstCustomFieldFilter.Add(new CustomFieldFilter { CustomFieldId = int.Parse(splittedCustomField[0]), OptionId = splittedCustomField[1] });
            //            lstFilteredCustomFieldOptionIds.Add(splittedCustomField[1]);
            //        };
            //        lstTacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, lstTacticIds);
            //        //// get Allowed Entity Ids
            //        lstTacticPer = lstTacticPer.Where(tactic => lstTacticIds.Contains(tactic.PlanTacticId)).ToList();
            //    }
            //    //// End - Added by Sohel Pathan on 16/01/2015 for PL ticket #1134
            //    //    //Modified by Komal Rawal for #1750 - For viewing onlly those tactic where user is owner, collaborator or have edit permission.
            //    //if (IsFiltered == false && customFieldIds == "" && ownerIds == "" && TacticTypeid == "" && StatusIds == "")
            //    {
            //        //Modified by Komal Rawal for #1750 - For viewing onlly those tactic where user is owner, collaborator or have edit permission.
            //        //List<string> collaboratorIds = Common.GetAllCollaborators(lstTacticIds).Distinct().ToList();
            //        //List<Guid> collaboratorIdsList = collaboratorIds.Select(Guid.Parse).ToList();
            //        //List<Guid> lstSubordinatesIds = new List<Guid>();
            //        //if (IsTacticAllowForSubordinates)
            //        //{
            //        //    lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
            //        //}
            //        //List<int> lsteditableEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);

            //        //Modified By Komal Rawal for #1505
            //        // Modified By Nishant Sheth 
            //        // Desc:: To resolve the #1790 observation
            //        //lstTactic = lstTactic.Where(tactic => tactic.CreatedBy == Sessions.User.UserId).Select(tactic => tactic).ToList();
            //    }
            //    //else
            //    {
            //        List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);
            //        //Modified By Komal Rawal for #1505
            //        // Modified By Nishant Sheth
            //        // Desc :: To resolve the select and deselct all Tactic type issues
            //        lstTacticPer = lstTacticPer.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || (filterOwner.Count.Equals(0) || filterOwner.Contains(tactic.CreatedBy))).Select(tactic => tactic).ToList();
            //    }
            //}

            //// Modified By Maninder Singh Wadhva PL Ticket#47
            //// Get list of Improvement tactics based on selected plan ids
            //List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic = objDbMrpEntities.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => filteredPlanIds.Contains(improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId) &&
            //                                                                          improvementTactic.IsDeleted.Equals(false) &&
            //                                                                          (improvementTactic.EffectiveDate > CalendarEndDate).Equals(false))
            //                                                                   .Select(improvementTactic => improvementTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

            //var lstSubordinatesWithPeers = new List<Guid>();
            //try
            //{
            //    //// Get list of subordinates and peer users
            //    lstSubordinatesWithPeers = Common.GetSubOrdinatesWithPeersNLevel();
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

            //// Get list of tactic and improvementTactic for which selected users are Owner
            //var subordinatesTactic = lstTacticPer.Where(tactic => lstSubordinatesWithPeers.Contains(tactic.CreatedBy)).ToList();
            //var subordinatesImprovementTactic = lstImprovementTactic.Where(improvementTactic => lstSubordinatesWithPeers.Contains(improvementTactic.CreatedBy)).ToList();

            //// Get Submitted tactic count for Request tab
            //string tacticStatus = Enums.TacticStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            //// Modified By Maninder Singh Wadhva PL Ticket#47, Modofied by Dharmraj #538
            //string requestCount = Convert.ToString(subordinatesTactic.Where(tactic => tactic.Status.Equals(tacticStatus)).Count() + subordinatesImprovementTactic.Where(improvementTactic => improvementTactic.Status.Equals(tacticStatus)).Count());



            //var tacticForAllTabs = lstTactic.ToList();

            //// Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
            //if (viewBy.Equals(PlanGanttTypes.Request.ToString()))
            //{
            //    lstTacticPer = subordinatesTactic;
            //    lstImprovementTactic = subordinatesImprovementTactic;
            //}

            //object improvementTacticForAccordion = new object();
            //object improvementTacticTypeForAccordion = new object();

            //// Added by Dharmraj Ticket #364,#365,#366
            //// Filter tactic and improvementTactic based on status and selected ViewBy option(tab)
            //if (objactivemenu.Equals(Enums.ActiveMenu.Plan))
            //{

            //    if (viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
            //    {
            //        IsRequest = true;
            //        List<string> status = GetStatusAsPerSelectedType(viewBy, objactivemenu);
            //        List<string> statusCD = new List<string>();
            //        statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            //        statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            //        lstTacticPer = lstTacticPer.Where(t => status.Contains(t.Status) || ((t.CreatedBy == Sessions.User.UserId && !viewBy.Equals(PlanGanttTypes.Request.ToString())) ? statusCD.Contains(t.Status) : false))
            //                        .Select(planTactic => planTactic).ToList();

            //        //List<string> improvementTacticStatus = GetStatusAsPerSelectedType(viewBy, objactivemenu);
            //        lstImprovementTactic = lstImprovementTactic.Where(improvementTactic => status.Contains(improvementTactic.Status) || ((improvementTactic.CreatedBy == Sessions.User.UserId && !viewBy.Equals(PlanGanttTypes.Request.ToString())) ? statusCD.Contains(improvementTactic.Status) : false))
            //                                                   .Select(improvementTactic => improvementTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();


            //    }
            //    else
            //    {

            //        lstTacticPer = lstTacticPer.Where(t => !viewBy.Equals(PlanGanttTypes.Request.ToString()))
            //                        .Select(planTactic => planTactic).ToList();

            //        if (IsFiltered && lstTacticPer.Count() == 0)
            //        {
            //            lstImprovementTactic = new List<Plan_Improvement_Campaign_Program_Tactic>();
            //        }
            //        else
            //        {
            //            lstImprovementTactic = lstImprovementTactic.Where(improvementTactic => improvementTactic.IsDeleted == false && !viewBy.Equals(PlanGanttTypes.Request.ToString()))
            //                                                       .Select(improvementTactic => improvementTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

            //        }

            //    }

            //}
            //else if (objactivemenu.Equals(Enums.ActiveMenu.Home))
            //{
            //    if (viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
            //    {
            //        IsRequest = true;
            //    }
            //    List<string> status = GetStatusAsPerSelectedType(viewBy, objactivemenu);
            //    List<string> statusCD = new List<string>();
            //    statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            //    statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            //    statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());
            //    lstTacticPer = lstTacticPer.Where(t => status.Contains(t.Status) || (!viewBy.Equals(PlanGanttTypes.Request.ToString()) ? statusCD.Contains(t.Status) : false)).Select(planTactic => planTactic).ToList();

            //    if (IsFiltered && lstTacticPer.Count() == 0)
            //    {
            //        lstImprovementTactic = new List<Plan_Improvement_Campaign_Program_Tactic>();
            //    }
            //    else
            //    {
            //        lstImprovementTactic = lstImprovementTactic.Where(improveTactic => status.Contains(improveTactic.Status) || (!viewBy.Equals(PlanGanttTypes.Request.ToString()) ? statusCD.Contains(improveTactic.Status) : false))
            //                                                   .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

            //    }
            //    //// Prepare objects for ImprovementTactic section of Left side accordian pane
            //    improvementTacticForAccordion = GetImprovementTacticForAccordion(lstImprovementTactic);
            //    improvementTacticTypeForAccordion = GetImprovementTacticTypeForAccordion(lstImprovementTactic);
            //}
            // Add By Nishant Sheth // Desc:: For get values from cache
            #endregion

            objCache.AddCache(Enums.CacheObject.ImprovementTactic.ToString(), lstImprovementTactic);
            //// Start - Added by Sohel Pathan on 28/10/2014 for PL ticket #885
            //// Prepare viewBy option list based on obtained tactic list

            //List<ViewByModel> viewByListResult = prepareViewByList(getViewByList, tacticForAllTabs);
            List<ViewByModel> viewByListResult = objSp.spViewByDropDownList(planId);
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
                    return PrepareTacticAndRequestTabResult(filteredPlanIds, viewBy, IsFiltered, IsRequest, objactivemenu, lstCampaign.ToList(), lstProgramPer.ToList(), lstTacticPer.ToList(), lstImprovementTactic, requestCount, planYear, improvementTacticForAccordion, improvementTacticTypeForAccordion, viewByListResult, filterOwner, filterStatus, lstPlans, timeFrame); // Modified By Nishant #1915
                }
                else
                {
                    return PrepareCustomFieldTabResult(viewBy, objactivemenu, lstCampaign.ToList(), lstProgramPer.ToList(), lstTacticPer.ToList(), lstImprovementTactic, requestCount, planYear, improvementTacticForAccordion, improvementTacticTypeForAccordion, viewByListResult, lstCustomFieldFilter, timeFrame); // Modified By Nishant
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
        private JsonResult PrepareCustomFieldTabResult(string viewBy, Enums.ActiveMenu activemenu, List<Plan_Campaign> lstCampaign, List<Custom_Plan_Campaign_Program> lstProgram, List<Custom_Plan_Campaign_Program_Tactic> lstTactic, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion, List<ViewByModel> viewByListResult, List<CustomFieldFilter> lstCustomFieldFilter, string timeframe = "")
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
            List<Custom_Plan_Campaign_Program_Tactic> tacticListByViewById = new List<Custom_Plan_Campaign_Program_Tactic>();
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
                lstStageIds = lstTactic.Select(tac => tac.StageId).Distinct().ToList();
                TacticStageMapping objStageTac;
                List<Custom_Plan_Campaign_Program_Tactic> lstPlanTactic;
                List<int> lstProgramIds, lstCampaignIds;
                DateTime minTacticDate = new DateTime();
                DateTime minProgramDate = new DateTime();
                DateTime minCampaignDate = new DateTime();
                List<Plan_Campaign_Program> programList;
                lstStageIds.ForEach(stgId =>
                {
                    lstPlanTactic = new List<Custom_Plan_Campaign_Program_Tactic>();
                    programList = new List<Plan_Campaign_Program>();
                    lstPlanTactic = lstTactic.Where(tatic => tatic.StageId == stgId).ToList();
                    lstProgramIds = lstCampaignIds = new List<int>();
                    lstProgramIds = lstPlanTactic.Select(tac => tac.PlanProgramId).ToList();
                    programList = Common.GetProgramFromCustomPreogramList(lstProgram.Where(prg => lstProgramIds.Contains(prg.PlanProgramId)).ToList());
                    lstCampaignIds = programList.Select(prg => prg.PlanCampaignId).ToList();
                    objStageTac = new TacticStageMapping();
                    objStageTac.StageId = stgId;
                    //objStageTac.TacticList = lstPlanTactic;
                    objStageTac.CustomTacticList = lstPlanTactic;
                    minTacticDate = lstPlanTactic.Select(tac => tac.StartDate).Min();
                    minProgramDate = programList.Select(prg => prg.StartDate).Min();
                    minCampaignDate = lstCampaign.Where(campgn => lstCampaignIds.Contains(campgn.PlanCampaignId)).Select(campgn => campgn.StartDate).Min();
                    objStageTac.minDate = new[] { minTacticDate, minProgramDate, minCampaignDate }.Min();
                    lstTacticStageMap.Add(objStageTac);
                });
                // Add By Nishant Sheth
                // Desc  :: For store Plan/Campagin/Program Progress DateTime #1798
                List<ProgressList> PlanProgresList = new List<ProgressList>();
                List<ProgressList> CampProgresList = new List<ProgressList>();
                List<ProgressList> ProgramProgresList = new List<ProgressList>();
                List<StartMinDatePlan> StartMinDatePlanList = new List<StartMinDatePlan>();

                // Commented By Nishant Sheth
                // Desc :: avoid foreach loop due to performance issue ticket #1798
                #region Old Code
                //foreach (var tacticItem in lstTactic)
                //{
                //    tacticstageId = tacticItem.objPlanTactic.StageId;
                //    tacticListByViewById = lstTacticStageMap.Where(tatic => tatic.StageId == tacticstageId).Select(tac => tac.TacticList).FirstOrDefault();
                //    tacticPlanId = tacticItem.PlanId;
                //    MinStartDateForCustomField = lstTacticStageMap.Where(tatic => tatic.StageId == tacticstageId).Select(tac => tac.minDate).FirstOrDefault();//GetMinStartDateStageAndBusinessUnit(lstCampaign, lstProgram, tacticListByViewById);

                //    //MinStartDateForPlan = GetMinStartDateForPlanOfCustomField(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById);
                //    #region  Minimum start date for plan
                //    var StartMinDatePlan = StartMinDatePlanList.Where(plan => plan.EntityId == tacticPlanId && plan.Status == tacticstatus).FirstOrDefault();
                //    if (StartMinDatePlan == null)
                //    {
                //        MinStartDateForPlan = GetMinStartDateForPlanOfCustomField(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById);
                //        StartMinDatePlanList.Add(new StartMinDatePlan { EntityId = tacticPlanId, Status = tacticstatus, StartDate = MinStartDateForPlan });

                //    }
                //    MinStartDateForPlan = StartMinDatePlanList.Where(plan => plan.EntityId == tacticPlanId && plan.Status == tacticstatus).FirstOrDefault().StartDate;

                //    #endregion

                //    EffectiveDateList = new List<DateTime>();
                //    if (EffectiveDateListByPlanIds != null && EffectiveDateListByPlanIds.Count > 0)
                //    {
                //        EffectiveDateList = EffectiveDateListByPlanIds.Where(_date => _date.PlanId == tacticPlanId).Select(_date => _date.EffectiveDate).ToList();
                //        if (EffectiveDateList != null && EffectiveDateList.Count > 0)
                //            minEffectiveDate = EffectiveDateList.Min();
                //    }
                //    // Add By Nishant Sheth
                //    // Desc :: To store Plan/Campaign/Program Progress becasue with same object not call progress function again #1798
                //    #region Plan Progress
                //    var PlanProgress = PlanProgresList.Where(plan => plan.EntityId == tacticPlanId).FirstOrDefault();
                //    if (PlanProgress == null)
                //    {
                //        PlanProgresList.Add(new ProgressList
                //        {
                //            EntityId = tacticPlanId,
                //            Progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlan),
                //                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlan,
                //                                    GetMaxEndDateForPlanOfCustomFields(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById)),
                //                                    tacticListByViewById, lstImprovementTactic, tacticPlanId)
                //        });
                //    }
                //    PlanProgress = PlanProgresList.Where(plan => plan.EntityId == tacticPlanId).FirstOrDefault();
                //    #endregion

                //    #region Campaign Progress
                //    var CampProgress = CampProgresList.Where(camp => camp.EntityId == tacticItem.objPlanTacticCampaign.PlanCampaignId).FirstOrDefault();
                //    if (CampProgress == null)
                //    {
                //        CampProgresList.Add(new ProgressList
                //        {
                //            EntityId = tacticItem.objPlanTacticCampaign.PlanCampaignId,
                //            Progress = GetCampaignProgressViewBy(tacticListByViewById, tacticItem.objPlanTacticCampaign, minEffectiveDate)
                //        });
                //    }
                //    CampProgress = CampProgresList.Where(camp => camp.EntityId == tacticItem.objPlanTacticCampaign.PlanCampaignId).FirstOrDefault();
                //    #endregion

                //    #region Program Progress
                //    var ProgramProgress = ProgramProgresList.Where(prog => prog.EntityId == tacticItem.objPlanTacticProgram.PlanProgramId).FirstOrDefault();
                //    if (ProgramProgress == null)
                //    {
                //        ProgramProgresList.Add(new ProgressList
                //        {
                //            EntityId = tacticItem.objPlanTacticProgram.PlanProgramId,
                //            Progress = GetProgramProgressViewBy(tacticListByViewById, tacticItem.objPlanTacticProgram, minEffectiveDate)
                //        });
                //    }
                //    ProgramProgress = ProgramProgresList.Where(prog => prog.EntityId == tacticItem.objPlanTacticProgram.PlanProgramId).FirstOrDefault();
                //    #endregion
                //    // End By Nishant Sheth
                //    lstTacticTaskList.Add(new TacticTaskList()
                //    {
                //        Tactic = tacticItem.objPlanTactic,
                //        PlanTactic = tacticItem,
                //        CreatedBy = tacticItem.CreatedBy,
                //        CustomFieldId = tacticstageId.ToString(),
                //        CustomFieldTitle = tacticItem.objPlanTactic.Stage.Title,
                //        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tacticstageId, tacticPlanId, tacticItem.PlanCampaignId, tacticItem.objPlanTactic.PlanProgramId, tacticItem.objPlanTactic.PlanTacticId),
                //        ColorCode = TacticColor,
                //        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                //        EndDate = DateTime.MaxValue,
                //        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField,
                //                                                GetMaxEndDateStageAndBusinessUnit(lstCampaign, lstProgram, tacticListByViewById)),
                //        CampaignProgress = CampProgress.Progress, // Modified by nishant sheth #1798
                //        ProgramProgress = ProgramProgress.Progress,// Modified by nishant sheth #1798
                //        PlanProgrss = PlanProgress.Progress,// Modified by nishant sheth #1798
                //        LinkTacticPermission = ((tacticItem.objPlanTactic.EndDate.Year - tacticItem.objPlanTactic.StartDate.Year) > 0) ? true : false,
                //        LinkedTacticId = tacticItem.objPlanTactic.LinkedTacticId
                //    });
                //}
                #endregion

                // Add By Nishant Sheth
                // Desc :: To increase performance #1798
                for (int i = 0; i < lstTactic.Count; i++)
                {
                    tacticstageId = lstTactic[i].StageId;
                    tacticListByViewById = lstTacticStageMap.Where(tatic => tatic.StageId == tacticstageId).Select(tac => tac.CustomTacticList).FirstOrDefault();
                    tacticPlanId = lstTactic[i].PlanId;
                    MinStartDateForCustomField = lstTacticStageMap.Where(tatic => tatic.StageId == tacticstageId).Select(tac => tac.minDate).FirstOrDefault();//GetMinStartDateStageAndBusinessUnit(lstCampaign, lstProgram, tacticListByViewById);

                    //MinStartDateForPlan = GetMinStartDateForPlanOfCustomField(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById);
                    #region  Minimum start date for plan
                    var StartMinDatePlan = StartMinDatePlanList.Where(plan => plan.EntityId == tacticPlanId && plan.Status == tacticstatus).FirstOrDefault();
                    if (StartMinDatePlan == null)
                    {
                        MinStartDateForPlan = GetMinStartDateForPlanOfCustomField(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById);
                        StartMinDatePlanList.Add(new StartMinDatePlan { EntityId = tacticPlanId, Status = tacticstatus, StartDate = MinStartDateForPlan });

                    }
                    MinStartDateForPlan = StartMinDatePlanList.Where(plan => plan.EntityId == tacticPlanId && plan.Status == tacticstatus).FirstOrDefault().StartDate;

                    #endregion

                    EffectiveDateList = new List<DateTime>();
                    if (EffectiveDateListByPlanIds != null && EffectiveDateListByPlanIds.Count > 0)
                    {
                        EffectiveDateList = EffectiveDateListByPlanIds.Where(_date => _date.PlanId == tacticPlanId).Select(_date => _date.EffectiveDate).ToList();
                        if (EffectiveDateList != null && EffectiveDateList.Count > 0)
                            minEffectiveDate = EffectiveDateList.Min();
                    }
                    // Add By Nishant Sheth
                    // Desc :: To store Plan/Campaign/Program Progress becasue with same object not call progress function again #1798
                    #region Plan Progress
                    var PlanProgress = PlanProgresList.Where(plan => plan.EntityId == tacticPlanId).FirstOrDefault();
                    if (PlanProgress == null)
                    {
                        PlanProgresList.Add(new ProgressList
                        {
                            EntityId = tacticPlanId,
                            Progress = GetProgressPerformance(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlan),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlan,
                                                    GetMaxEndDateForPlanOfCustomFields(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById)),
                                                    tacticListByViewById, lstImprovementTactic, tacticPlanId)
                        });
                    }
                    PlanProgress = PlanProgresList.Where(plan => plan.EntityId == tacticPlanId).FirstOrDefault();
                    #endregion

                    #region Campaign Progress
                    var CampProgress = CampProgresList.Where(camp => camp.EntityId == lstTactic[i].PlanCampaignId).FirstOrDefault();
                    if (CampProgress == null)
                    {
                        CampProgresList.Add(new ProgressList
                        {
                            EntityId = lstTactic[i].PlanCampaignId,
                            Progress = GetCampaignProgressViewByPerformance(tacticListByViewById, lstCampaign.Where(a => a.PlanCampaignId == lstTactic[i].PlanCampaignId).FirstOrDefault(), minEffectiveDate)
                        });
                    }
                    CampProgress = CampProgresList.Where(camp => camp.EntityId == lstTactic[i].PlanCampaignId).FirstOrDefault();
                    #endregion

                    #region Program Progress
                    var ProgramProgress = ProgramProgresList.Where(prog => prog.EntityId == lstTactic[i].PlanProgramId).FirstOrDefault();
                    if (ProgramProgress == null)
                    {
                        ProgramProgresList.Add(new ProgressList
                        {
                            EntityId = lstTactic[i].PlanProgramId,
                            Progress = GetProgramProgressViewByPerformance(tacticListByViewById, lstProgram.Where(a => a.PlanProgramId == lstTactic[i].PlanProgramId).FirstOrDefault(), minEffectiveDate)
                        });
                    }
                    ProgramProgress = ProgramProgresList.Where(prog => prog.EntityId == lstTactic[i].PlanProgramId).FirstOrDefault();
                    #endregion
                    // End By Nishant Sheth
                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = Common.GetTacticFromCustomTacticModel(lstTactic[i]),
                        //PlanTactic = lstTactic[i],
                        CustomTactic = lstTactic[i],
                        CreatedBy = lstTactic[i].CreatedBy,
                        CustomFieldId = tacticstageId.ToString(),
                        CustomFieldTitle = lstTactic[i].StageTitle,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tacticstageId, tacticPlanId, lstTactic[i].PlanCampaignId, lstTactic[i].PlanProgramId, lstTactic[i].PlanTacticId),
                        ColorCode = TacticColor,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                        EndDate = DateTime.MaxValue,
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField,
                                                                GetMaxEndDateStageAndBusinessUnitPerformance(lstCampaign, lstProgram, tacticListByViewById)),
                        CampaignProgress = CampProgress.Progress, // Modified by nishant sheth #1798
                        ProgramProgress = ProgramProgress.Progress,// Modified by nishant sheth #1798
                        PlanProgrss = PlanProgress.Progress,// Modified by nishant sheth #1798
                        LinkTacticPermission = ((lstTactic[i].EndDate.Year - lstTactic[i].StartDate.Year) > 0) ? true : false,
                        LinkedTacticId = lstTactic[i].LinkedTacticId
                    });
                }

                //// Prepare stage list for Marketting accrodian for Home screen only.
                if (activemenu.Equals(Enums.ActiveMenu.Home))
                {
                    lstCustomFields = (lstTactic.Select(tactic => tactic)).Select(tactic => new
                    {
                        CustomFieldId = tactic.StageId,
                        Title = tactic.StageTitle,
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
                                             join tactic in lstTactic on improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals tactic.PlanId
                                             join stage in objDbMrpEntities.Stages on tactic.StageId equals stage.StageId
                                             where improvementTactic.IsDeleted == false && tactic.IsDeleted == false && stage.IsDeleted == false
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
                lstStatus = lstTactic.Select(tac => tac.Status).Distinct().ToList();
                TacticStageMapping objStageTac;
                List<Custom_Plan_Campaign_Program_Tactic> lstPlanTactic;
                List<int> lstProgramIds, lstCampaignIds;
                DateTime minTacticDate = new DateTime();
                DateTime minProgramDate = new DateTime();
                DateTime minCampaignDate = new DateTime();
                List<Plan_Campaign_Program> programList;
                lstStatus.ForEach(stats =>
                {
                    lstPlanTactic = new List<Custom_Plan_Campaign_Program_Tactic>();
                    programList = new List<Plan_Campaign_Program>();
                    //lstPlanTactic = lstTactic.Where(tatic => tatic.Status == stats).ToList();
                    lstPlanTactic = lstTactic.Where(tatic => tatic.Status == stats).ToList();
                    lstProgramIds = lstCampaignIds = new List<int>();
                    lstProgramIds = lstPlanTactic.Select(tac => tac.PlanProgramId).ToList();
                    programList = Common.GetProgramFromCustomPreogramList(lstProgram.Where(prg => lstProgramIds.Contains(prg.PlanProgramId)).ToList());
                    lstCampaignIds = programList.Select(prg => prg.PlanCampaignId).ToList();
                    objStageTac = new TacticStageMapping();
                    objStageTac.Status = stats;
                    //objStageTac.TacticList = lstPlanTactic;
                    objStageTac.CustomTacticList = lstPlanTactic;
                    minTacticDate = lstPlanTactic.Select(tac => tac.StartDate).Min();
                    minProgramDate = programList.Select(prg => prg.StartDate).Min();
                    minCampaignDate = lstCampaign.Where(campgn => lstCampaignIds.Contains(campgn.PlanCampaignId)).Select(campgn => campgn.StartDate).Min();
                    objStageTac.minDate = new[] { minTacticDate, minProgramDate, minCampaignDate }.Min();
                    lstTacticStageMap.Add(objStageTac);
                });
                #endregion
                // Add By Nishant Sheth
                // Desc  :: For store Plan/Campagin/Program Progress DateTime #1798
                List<ProgressList> PlanProgresList = new List<ProgressList>();
                List<ProgressList> CampProgresList = new List<ProgressList>();
                List<ProgressList> ProgramProgresList = new List<ProgressList>();
                List<StartMinDatePlan> StartMinDatePlanList = new List<StartMinDatePlan>();

                // Commented By Nishant Sheth
                // Desc :: avoid foreach loop due to performance issue ticket #1798
                #region Old Code
                //foreach (var tacticItem in lstTactic)
                //{
                //    tacticstatus = tacticItem.objPlanTactic.Status;
                //    tacticListByViewById = lstTacticStageMap.Where(tatic => tatic.Status == tacticstatus).Select(tatic => tatic.TacticList).FirstOrDefault();
                //    tacticPlanId = tacticItem.PlanId;
                //    MinStartDateForCustomField = lstTacticStageMap.Where(tatic => tatic.Status == tacticstatus).Select(tac => tac.minDate).FirstOrDefault();//GetMinStartDateStageAndBusinessUnit(lstCampaign, lstProgram, tacticListByViewById);
                //    #region  Minimum start date for plan
                //    var StartMinDatePlan = StartMinDatePlanList.Where(plan => plan.EntityId == tacticPlanId && plan.Status == tacticstatus).FirstOrDefault();
                //    if (StartMinDatePlan == null)
                //    {
                //        MinStartDateForPlan = GetMinStartDateForPlanOfCustomField(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById);
                //        StartMinDatePlanList.Add(new StartMinDatePlan { EntityId = tacticPlanId, Status = tacticstatus, StartDate = MinStartDateForPlan });

                //    }
                //    MinStartDateForPlan = StartMinDatePlanList.Where(plan => plan.EntityId == tacticPlanId && plan.Status == tacticstatus).FirstOrDefault().StartDate;

                //    #endregion
                //    EffectiveDateList = new List<DateTime>();
                //    if (EffectiveDateListByPlanIds != null && EffectiveDateListByPlanIds.Count > 0)
                //    {
                //        EffectiveDateList = EffectiveDateListByPlanIds.Where(_date => _date.PlanId == tacticPlanId).Select(_date => _date.EffectiveDate).ToList();
                //        if (EffectiveDateList != null && EffectiveDateList.Count > 0)
                //            minEffectiveDate = EffectiveDateList.Min();
                //    }

                //    // Add By Nishant Sheth
                //    // Desc :: To store Plan/Campaign/Program Progress becasue with same object not call progress function again #1798
                //    #region Plan Progress
                //    var PlanProgress = PlanProgresList.Where(plan => plan.EntityId == tacticPlanId).FirstOrDefault();
                //    if (PlanProgress == null)
                //    {
                //        PlanProgresList.Add(new ProgressList
                //        {
                //            EntityId = tacticPlanId,
                //            Progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlan),
                //                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlan,
                //                                    GetMaxEndDateForPlanOfCustomFields(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById)),
                //                                    tacticListByViewById, lstImprovementTactic, tacticPlanId)
                //        });
                //    }
                //    PlanProgress = PlanProgresList.Where(plan => plan.EntityId == tacticPlanId).FirstOrDefault();
                //    #endregion

                //    #region Campaign Progress
                //    var CampProgress = CampProgresList.Where(camp => camp.EntityId == tacticItem.objPlanTacticCampaign.PlanCampaignId).FirstOrDefault();
                //    if (CampProgress == null)
                //    {
                //        CampProgresList.Add(new ProgressList
                //        {
                //            EntityId = tacticItem.objPlanTacticCampaign.PlanCampaignId,
                //            Progress = GetCampaignProgressViewBy(tacticListByViewById, tacticItem.objPlanTacticCampaign, minEffectiveDate)
                //        });
                //    }
                //    CampProgress = CampProgresList.Where(camp => camp.EntityId == tacticItem.objPlanTacticCampaign.PlanCampaignId).FirstOrDefault();
                //    #endregion

                //    #region Program Progress
                //    var ProgramProgress = ProgramProgresList.Where(prog => prog.EntityId == tacticItem.objPlanTacticProgram.PlanProgramId).FirstOrDefault();
                //    if (ProgramProgress == null)
                //    {
                //        ProgramProgresList.Add(new ProgressList
                //        {
                //            EntityId = tacticItem.objPlanTacticProgram.PlanProgramId,
                //            Progress = GetProgramProgressViewBy(tacticListByViewById, tacticItem.objPlanTacticProgram, minEffectiveDate)
                //        });
                //    }
                //    ProgramProgress = ProgramProgresList.Where(prog => prog.EntityId == tacticItem.objPlanTacticProgram.PlanProgramId).FirstOrDefault();
                //    #endregion
                //    // End By Nishant Sheth

                //    lstTacticTaskList.Add(new TacticTaskList()
                //    {

                //        Tactic = tacticItem.objPlanTactic,
                //        PlanTactic = tacticItem,
                //        CreatedBy = tacticItem.CreatedBy,
                //        CustomFieldId = tacticstatus,
                //        CustomFieldTitle = tacticstatus,
                //        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tacticstatus, tacticPlanId, tacticItem.PlanCampaignId, tacticItem.objPlanTactic.PlanProgramId, tacticItem.objPlanTactic.PlanTacticId),
                //        ColorCode = TacticColor,
                //        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                //        EndDate = DateTime.MaxValue,
                //        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField,
                //                                                GetMaxEndDateStageAndBusinessUnit(lstCampaign, lstProgram, tacticListByViewById)),
                //        //CampaignProgress = GetCampaignProgressViewBy(tacticListByViewById, tacticItem.objPlanTacticCampaign, minEffectiveDate),
                //        CampaignProgress = CampProgress.Progress, // Modified By Nishant Sheth #1798
                //        //ProgramProgress = GetProgramProgressViewBy(tacticListByViewById, tacticItem.objPlanTacticProgram, minEffectiveDate),
                //        ProgramProgress = ProgramProgress.Progress,// Modified By Nishant Sheth #1798
                //        //PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlan),
                //        //                            Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlan,
                //        //                            GetMaxEndDateForPlanOfCustomFields(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById)),
                //        //                            tacticListByViewById, lstImprovementTactic, tacticPlanId),
                //        PlanProgrss = PlanProgress.Progress,// Modified By Nishant Sheth #1798
                //        LinkTacticPermission = ((tacticItem.objPlanTactic.EndDate.Year - tacticItem.objPlanTactic.StartDate.Year) > 0) ? true : false,
                //        LinkedTacticId = tacticItem.objPlanTactic.LinkedTacticId
                //    });
                //}
                #endregion

                // Add By Nishant Sheth
                // Desc :: To increase performance #1798
                for (int i = 0; i < lstTactic.Count; i++)
                {
                    tacticstatus = lstTactic[i].Status;
                    tacticListByViewById = lstTacticStageMap.Where(tatic => tatic.Status == tacticstatus).Select(tatic => tatic.CustomTacticList).FirstOrDefault();
                    tacticPlanId = lstTactic[i].PlanId;
                    MinStartDateForCustomField = lstTacticStageMap.Where(tatic => tatic.Status == tacticstatus).Select(tac => tac.minDate).FirstOrDefault();//GetMinStartDateStageAndBusinessUnit(lstCampaign, lstProgram, tacticListByViewById);
                    #region  Minimum start date for plan
                    var StartMinDatePlan = StartMinDatePlanList.Where(plan => plan.EntityId == tacticPlanId && plan.Status == tacticstatus).FirstOrDefault();
                    if (StartMinDatePlan == null)
                    {
                        MinStartDateForPlan = GetMinStartDateForPlanOfCustomField(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById);
                        StartMinDatePlanList.Add(new StartMinDatePlan { EntityId = tacticPlanId, Status = tacticstatus, StartDate = MinStartDateForPlan });

                    }
                    MinStartDateForPlan = StartMinDatePlanList.Where(plan => plan.EntityId == tacticPlanId && plan.Status == tacticstatus).FirstOrDefault().StartDate;

                    #endregion
                    EffectiveDateList = new List<DateTime>();
                    if (EffectiveDateListByPlanIds != null && EffectiveDateListByPlanIds.Count > 0)
                    {
                        EffectiveDateList = EffectiveDateListByPlanIds.Where(_date => _date.PlanId == tacticPlanId).Select(_date => _date.EffectiveDate).ToList();
                        if (EffectiveDateList != null && EffectiveDateList.Count > 0)
                            minEffectiveDate = EffectiveDateList.Min();
                    }

                    // Add By Nishant Sheth
                    // Desc :: To store Plan/Campaign/Program Progress becasue with same object not call progress function again #1798
                    #region Plan Progress
                    var PlanProgress = PlanProgresList.Where(plan => plan.EntityId == tacticPlanId).FirstOrDefault();
                    if (PlanProgress == null)
                    {
                        PlanProgresList.Add(new ProgressList
                        {
                            EntityId = tacticPlanId,
                            Progress = GetProgressPerformance(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlan),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlan,
                                                    GetMaxEndDateForPlanOfCustomFields(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById)),
                                                    tacticListByViewById, lstImprovementTactic, tacticPlanId)
                        });
                    }
                    PlanProgress = PlanProgresList.Where(plan => plan.EntityId == tacticPlanId).FirstOrDefault();
                    #endregion

                    #region Campaign Progress
                    var CampProgress = CampProgresList.Where(camp => camp.EntityId == lstTactic[i].PlanCampaignId).FirstOrDefault();
                    if (CampProgress == null)
                    {
                        CampProgresList.Add(new ProgressList
                        {
                            EntityId = lstTactic[i].PlanCampaignId,
                            Progress = GetCampaignProgressViewByPerformance(tacticListByViewById, lstCampaign.Where(a => a.PlanCampaignId == lstTactic[i].PlanCampaignId).FirstOrDefault(), minEffectiveDate)
                        });
                    }
                    CampProgress = CampProgresList.Where(camp => camp.EntityId == lstTactic[i].PlanCampaignId).FirstOrDefault();
                    #endregion

                    #region Program Progress
                    var ProgramProgress = ProgramProgresList.Where(prog => prog.EntityId == lstTactic[i].PlanProgramId).FirstOrDefault();
                    if (ProgramProgress == null)
                    {
                        ProgramProgresList.Add(new ProgressList
                        {
                            EntityId = lstTactic[i].PlanProgramId,
                            Progress = GetProgramProgressViewByPerformance(tacticListByViewById, lstProgram.Where(a => a.PlanProgramId == lstTactic[i].PlanProgramId).FirstOrDefault(), minEffectiveDate)
                        });
                    }
                    ProgramProgress = ProgramProgresList.Where(prog => prog.EntityId == lstTactic[i].PlanProgramId).FirstOrDefault();
                    #endregion
                    // End By Nishant Sheth

                    lstTacticTaskList.Add(new TacticTaskList()
                    {

                        Tactic = Common.GetTacticFromCustomTacticModel(lstTactic[i]),
                        //PlanTactic = lstTactic[i],
                        CustomTactic = lstTactic[i],
                        CreatedBy = lstTactic[i].CreatedBy,
                        CustomFieldId = tacticstatus,
                        CustomFieldTitle = tacticstatus,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tacticstatus, tacticPlanId, lstTactic[i].PlanCampaignId, lstTactic[i].PlanProgramId, lstTactic[i].PlanTacticId),
                        ColorCode = TacticColor,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                        EndDate = DateTime.MaxValue,
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField,
                                                                GetMaxEndDateStageAndBusinessUnitPerformance(lstCampaign, lstProgram, tacticListByViewById)),
                        //CampaignProgress = GetCampaignProgressViewBy(tacticListByViewById, tacticItem.objPlanTacticCampaign, minEffectiveDate),
                        CampaignProgress = CampProgress.Progress, // Modified By Nishant Sheth #1798
                        //ProgramProgress = GetProgramProgressViewBy(tacticListByViewById, tacticItem.objPlanTacticProgram, minEffectiveDate),
                        ProgramProgress = ProgramProgress.Progress,// Modified By Nishant Sheth #1798
                        //PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlan),
                        //                            Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlan,
                        //                            GetMaxEndDateForPlanOfCustomFields(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById)),
                        //                            tacticListByViewById, lstImprovementTactic, tacticPlanId),
                        PlanProgrss = PlanProgress.Progress,// Modified By Nishant Sheth #1798
                        LinkTacticPermission = ((lstTactic[i].EndDate.Year - lstTactic[i].StartDate.Year) > 0) ? true : false,
                        LinkedTacticId = lstTactic[i].LinkedTacticId
                    });
                }

                //// Prepare  list for Marketting accrodian for Home screen only.
                if (activemenu.Equals(Enums.ActiveMenu.Home))
                {
                    lstCustomFields = (lstTactic.Select(tactic => tactic.Status)).Select(tactic => new
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
                                             join tactic in lstTactic on improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals tactic.PlanId
                                             where improvementTactic.IsDeleted == false && tactic.IsDeleted == false && improvementTactic.Status == tactic.Status
                                             select new ImprovementTaskDetail()
                                             {
                                                 ImprovementTactic = improvementTactic,
                                                 MainParentId = tactic.Status,
                                                 MinStartDate = lstImprovementTactic.Where(impTactic => impTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impTactic => impTactic.EffectiveDate).Min(),
                                             }).Distinct().ToList();
            }

            //// Prepare task tactic list for CustomFields tab(ViewBy)
            else
            {
                //// Get list of tactic ids from tactic list
                //Commented By Bhavesh because it lake more time and give timeout expire error : Start Date: 30-10-2015 #1726
                List<int> entityids = lstTactic.Select(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.PlanProgramId : tactic.PlanTacticId))).ToList();
                CustomFieldOption objCustomFieldOption = new CustomFieldOption();
                objCustomFieldOption.CustomFieldOptionId = 0;
                string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
                // Comment by nishant sheth
                // Desc :: To Remove db trips

                string customFieldType = objDbMrpEntities.CustomFields.Where(c => c.CustomFieldId == CustomTypeId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                //var cusomfieldEntity = objDbMrpEntities.CustomField_Entity.Where(c => c.CustomFieldId == CustomTypeId && entityids.Contains(c.EntityId)).ToList();

                DataSet dsCustomFieldEntity = new DataSet();
                var EntityType = IsCampaign ? Enums.Section.Campaign.ToString() : (IsProgram ? Enums.Section.Program.ToString() : Enums.Section.Tactic.ToString());
                dsCustomFieldEntity = objSp.GetCustomFieldEntityList(EntityType: EntityType, CustomTypeId: CustomTypeId);
                var cusomfieldEntity = Common.GetSpCustomFieldEntityList(dsCustomFieldEntity.Tables[1])
                    .Where(c => entityids.Contains(c.EntityId))
                    .ToList();

                // Add by nishant sheth
                // Desc :: Get data from cache for performance
                //string customFieldType = ((List<CustomField>)objCache.Returncache(Enums.CacheObject.CustomField.ToString())).Where(c => c.CustomFieldId == CustomTypeId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                //var cusomfieldEntity = ((List<CacheCustomField>)objCache.Returncache(Enums.CacheObject.CustomFieldEntity.ToString())).Where(c => c.CustomFieldId == CustomTypeId && entityids.Contains(c.EntityId)).ToList();

                var customoptionlisttest = (from cfo in objDbMrpEntities.CustomFieldOptions
                                            where cfo.CustomFieldId == CustomTypeId && cfo.IsDeleted == false
                                            select new
                                            {
                                                CustomFieldId = cfo.CustomFieldId,
                                                CustomFieldOptionId = cfo.CustomFieldOptionId,
                                                Title = cfo.Value
                                            }).ToList();

                // Update By Bhavesh Ticket #1798 Date : 05-Jan-2016 : Remove condition from list and get only one time
                var filtercustomfieldoptionid = lstCustomFieldFilter.Where(custmlst => custmlst.CustomFieldId.Equals(CustomTypeId)).Select(custmlst => custmlst.OptionId).ToList();
                bool isfilteroption = (lstCustomFieldFilter.Where(custmlst => custmlst.CustomFieldId.Equals(CustomTypeId)).Any());
                var lstCustomFieldTactic = (from customfieldentity in cusomfieldEntity
                                                //join tactic in lstTactic on customfieldentity.EntityId equals tactic.objPlanTactic.PlanTacticId   //Commenetd by Rahul Shah on 17/11/2015 for PL #1760. Bcz in this condition Campaign and Program data not displyed.
                                            join tactic in lstTactic on customfieldentity.EntityId equals (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.PlanProgramId : tactic.PlanTacticId)) //Added by Rahul Shah on 17/11/2015 for PL #1760. It Will Check Campaign And Program Field data 
                                            select new
                                            {
                                                tactic = tactic,
                                                masterCustomFieldId = CustomTypeId,
                                                customFieldId = customFieldType == DropDownList ? (customoptionlisttest.Count() == 0 ? 0 : Convert.ToInt32(customfieldentity.Value)) : customfieldentity.CustomFieldEntityId,
                                                customFieldTitle = customFieldType == DropDownList ? customoptionlisttest.Where(c => c.CustomFieldOptionId.ToString() == customfieldentity.Value).Select(c => c.Title).FirstOrDefault() : customfieldentity.Value,
                                                customFieldTYpe = customFieldType,
                                                EntityId = customfieldentity.EntityId,
                                                CreatedBy = customfieldentity.CreatedBy
                                            }).ToList().Where(fltr => (fltr.customFieldTYpe == DropDownList ? (!isfilteroption || filtercustomfieldoptionid.Contains(fltr.customFieldId.ToString())) && fltr.customFieldId != 0 && fltr.customFieldTitle != null : true)).Distinct().ToList();

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
                    customFieldId = customFieldTactic.customFieldId,
                    customFieldTitle = customFieldTactic.customFieldTitle

                }).ToList();

                TacticStageMapping objStageTac;
                var lstUniqueCustomField = lstProcessedCustomFieldTactics.Select(c => c.customFieldId).Distinct().ToList();
                List<Custom_Plan_Campaign_Program_Tactic> lstPlanTactic;
                lstUniqueCustomField.ForEach(cstmId =>
                {
                    objStageTac = new TacticStageMapping();
                    lstPlanTactic = new List<Custom_Plan_Campaign_Program_Tactic>();
                    lstPlanTactic = lstProcessedCustomFieldTactics.Where(ct => ct.customFieldId == cstmId).Select(ct => ct.tactic).ToList();
                    objStageTac.CustomFieldId = cstmId;
                    //objStageTac.TacticList = lstPlanTactic;
                    objStageTac.CustomTacticList = lstPlanTactic;
                    lstTacticStageMap.Add(objStageTac);
                });

                DateTime MaxEndDateForCustomField;
                int _PlanTacticId = 0, _PlanProgramId = 0, _TypeId = 0;
                List<Custom_Plan_Campaign_Program_Tactic> fltrTactic;
                //// Prepare tactic task list for CustomField to be used in rendering of gantt chart. 
                List<int> PlanIds = lstProcessedCustomFieldTactics.Select(_tac => _tac.tactic.PlanId).ToList();
                List<ProgressModel> EffectiveDateListByPlanIds = lstImprovementTactic.Where(imprvmnt => PlanIds.Contains(imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).Select(imprvmnt => new ProgressModel { PlanId = imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, EffectiveDate = imprvmnt.EffectiveDate }).ToList();
                DateTime maxDateCampaign, maxDateProgram, maxDateTactic, minDateCampaign, minDateProgram, minDateTactic;

                List<PlanMinMaxDate> planminmaxdatelist = new List<PlanMinMaxDate>();
                PlanMinMaxDate planminmaxdateobj = new PlanMinMaxDate();
                foreach (var lts in lstTacticStageMap)
                {
                    var tacticplanids = lts.CustomTacticList.Select(t => t.PlanId).Distinct().ToList();
                    foreach (var tpl in tacticplanids)
                    {
                        planminmaxdateobj = new PlanMinMaxDate();
                        minEffectiveDate = new DateTime();
                        var filplantacticlist = lts.CustomTacticList.Where(t => t.PlanId == tpl).ToList();
                        EffectiveDateList = new List<DateTime>();
                        if (EffectiveDateListByPlanIds != null && EffectiveDateListByPlanIds.Count > 0)
                        {
                            EffectiveDateList = EffectiveDateListByPlanIds.Where(_date => _date.PlanId == tpl).Select(_date => _date.EffectiveDate).ToList();
                            if (EffectiveDateList != null && EffectiveDateList.Count > 0)
                                minEffectiveDate = EffectiveDateList.Min();
                        }
                        var tacProgramId = filplantacticlist.Select(a => a.PlanProgramId).ToList();
                        var tacCampId = filplantacticlist.Select(a => a.PlanCampaignId).ToList();
                        var tacProgram = lstProgram.Where(a => tacProgramId.Contains(a.PlanProgramId)).ToList();
                        var tacCampaign = lstCampaign.Where(a => tacCampId.Contains(a.PlanCampaignId)).ToList();
                        #region "Get Min Start Date of Plan"
                        minDateTactic = minDateProgram = minDateCampaign = new DateTime();
                        minDateTactic = filplantacticlist.Select(tactic => tactic.StartDate).Min();
                        minDateProgram = tacProgram.Select(prog => prog.StartDate).Min();
                        minDateCampaign = tacCampaign.Select(camp => camp.StartDate).Min();
                        MinStartDateForPlan = new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
                        #endregion

                        #region "Get Max End Date of Plan"
                        maxDateTactic = minDateProgram = minDateCampaign = new DateTime();
                        maxDateTactic = filplantacticlist.Select(tactic => tactic.EndDate).Max();
                        maxDateProgram = tacProgram.Select(prog => prog.EndDate).Max();
                        maxDateCampaign = tacCampaign.Select(camp => camp.EndDate).Max();
                        MaxEndDateForPlan = new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
                        #endregion
                        planminmaxdateobj.PlanId = tpl;
                        planminmaxdateobj.CustomFieldId = lts.CustomFieldId;
                        planminmaxdateobj.minEffectiveDate = minEffectiveDate;
                        planminmaxdateobj.minDate = MinStartDateForPlan;
                        planminmaxdateobj.maxDate = MaxEndDateForPlan;
                        planminmaxdatelist.Add(planminmaxdateobj);
                    }
                }
                // Add By Nishant Sheth
                // Desc  :: For store Plan/Campagin/Program Progress DateTime #1798
                List<ProgressList> PlanProgresList = new List<ProgressList>();
                List<ProgressList> CampProgresList = new List<ProgressList>();
                List<ProgressList> ProgramProgresList = new List<ProgressList>();
                // Commented By Nishant Sheth
                // Desc :: avoid foreach loop due to performance issue ticket #1798
                #region Old Code
                //foreach (var tacticItem in lstProcessedCustomFieldTactics)
                //{
                //    tacticPlanId = tacticItem.tactic.PlanId;
                //    tacticPlanCampaignId = tacticItem.tactic.PlanCampaignId;
                //    _PlanTacticId = tacticItem.tactic.objPlanTactic.PlanTacticId;
                //    _PlanProgramId = tacticItem.tactic.objPlanTactic.PlanProgramId;
                //    _TypeId = IsCampaign ? tacticPlanCampaignId : (IsProgram ? _PlanProgramId : _PlanTacticId);

                //    tacticListByViewById = lstTacticStageMap.Where(c => c.CustomFieldId == tacticItem.customFieldId).Select(c => c.TacticList).FirstOrDefault();//lstTactic.Where(tactic => tacticItem.customFieldTacticList.Contains(IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId))).Select(tactic => tactic).ToList();
                //    planminmaxdateobj = new PlanMinMaxDate();
                //    planminmaxdateobj = planminmaxdatelist.Where(p => p.CustomFieldId == tacticItem.customFieldId && p.PlanId == tacticPlanId).FirstOrDefault();
                //    #region "Get CampaignIds,ProgramIds & Tactic list for Min & Max Date"
                //    fltrTactic = new List<Plan_Tactic>();
                //    fltrTactic = tacticListByViewById.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId)) == _TypeId).ToList();

                //    #endregion
                //    #region "Get Tactic,Program & Campaign MaxDate"
                //    maxDateTactic = fltrTactic.Select(tactic => tactic.objPlanTactic.EndDate).Max();
                //    maxDateProgram = fltrTactic.Select(tactic => tactic.objPlanTacticProgram.EndDate).Max();
                //    maxDateCampaign = fltrTactic.Select(tactic => tactic.objPlanTacticCampaign.EndDate).Max();
                //    MaxEndDateForCustomField = new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
                //    #endregion
                //    #region "Get Tactic,Program & Campaign MinDate"
                //    minDateTactic = new DateTime();
                //    minDateTactic = fltrTactic.Select(tactic => tactic.objPlanTactic.StartDate).Min();
                //    minDateProgram = fltrTactic.Select(tactic => tactic.objPlanTacticProgram.StartDate).Min();
                //    minDateCampaign = fltrTactic.Select(tactic => tactic.objPlanTacticCampaign.StartDate).Min();
                //    MinStartDateForCustomField = new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
                //    #endregion


                //    // Add By Nishant Sheth
                //    // Desc :: To store Plan/Campaign/Program Progress becasue with same object not call progress function again #1798
                //    #region Plan Progress
                //    var PlanProgress = PlanProgresList.Where(plan => plan.EntityId == tacticPlanId).FirstOrDefault();
                //    if (PlanProgress == null)
                //    {
                //        PlanProgresList.Add(new ProgressList
                //        {
                //            EntityId = tacticPlanId,
                //            Progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, planminmaxdateobj.minDate),
                //                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, planminmaxdateobj.minDate,
                //                                    GetMaxEndDateForPlanOfCustomFields(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById)),
                //                                    tacticListByViewById, lstImprovementTactic, tacticPlanId)
                //        });
                //    }
                //    PlanProgress = PlanProgresList.Where(plan => plan.EntityId == tacticPlanId).FirstOrDefault();
                //    #endregion

                //    #region Campaign Progress
                //    var CampProgress = CampProgresList.Where(camp => camp.EntityId == tacticItem.tactic.objPlanTacticCampaign.PlanCampaignId).FirstOrDefault();
                //    if (CampProgress == null)
                //    {
                //        CampProgresList.Add(new ProgressList
                //        {
                //            EntityId = tacticItem.tactic.objPlanTacticCampaign.PlanCampaignId,
                //            Progress = GetCampaignProgressViewBy(tacticListByViewById, tacticItem.tactic.objPlanTacticCampaign, planminmaxdateobj.minEffectiveDate)
                //        });
                //    }
                //    CampProgress = CampProgresList.Where(camp => camp.EntityId == tacticItem.tactic.objPlanTacticCampaign.PlanCampaignId).FirstOrDefault();
                //    #endregion

                //    #region Program Progress
                //    var ProgramProgress = ProgramProgresList.Where(prog => prog.EntityId == tacticItem.tactic.objPlanTacticProgram.PlanProgramId).FirstOrDefault();
                //    if (ProgramProgress == null)
                //    {
                //        ProgramProgresList.Add(new ProgressList
                //        {
                //            EntityId = tacticItem.tactic.objPlanTacticProgram.PlanProgramId,
                //            Progress = GetProgramProgressViewBy(tacticListByViewById, tacticItem.tactic.objPlanTacticProgram, planminmaxdateobj.minEffectiveDate)
                //        });
                //    }
                //    ProgramProgress = ProgramProgresList.Where(prog => prog.EntityId == tacticItem.tactic.objPlanTacticProgram.PlanProgramId).FirstOrDefault();
                //    #endregion
                //    // End By Nishant Sheth

                //    lstTacticTaskList.Add(new TacticTaskList()
                //    {
                //        Tactic = tacticItem.tactic.objPlanTactic,
                //        PlanTactic = tacticItem.tactic,
                //        CustomFieldId = tacticItem.customFieldId.ToString(),
                //        CustomFieldTitle = tacticItem.customFieldTitle,
                //        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tacticItem.customFieldId, tacticPlanId, tacticPlanCampaignId, _PlanProgramId, _PlanTacticId),
                //        ColorCode = TacticColor,
                //        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                //        EndDate = Common.GetEndDateAsPerCalendarInDateFormat(CalendarEndDate, MaxEndDateForCustomField),
                //        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField, MaxEndDateForCustomField),
                //        CampaignProgress = CampProgress.Progress, // Modified By Nishant sheth #1798
                //        ProgramProgress = ProgramProgress.Progress,// Modified By Nishant sheth #1798
                //        PlanProgrss = PlanProgress.Progress,// Modified By Nishant sheth #1798
                //        CreatedBy = tacticItem.CreatedBy,
                //        PlanStartDate = planminmaxdateobj.minDate,
                //        PlanEndDate = planminmaxdateobj.maxDate,
                //        LinkTacticPermission = ((tacticItem.tactic.objPlanTactic.EndDate.Year - tacticItem.tactic.objPlanTactic.StartDate.Year) > 0) ? true : false,
                //        LinkedTacticId = tacticItem.tactic.objPlanTactic.LinkedTacticId
                //    });
                //}
                #endregion

                // Add By Nishant Sheth
                // Desc :: To increase performance #1798
                for (int i = 0; i < lstProcessedCustomFieldTactics.Count; i++)
                {
                    tacticPlanId = lstProcessedCustomFieldTactics[i].tactic.PlanId;
                    tacticPlanCampaignId = lstProcessedCustomFieldTactics[i].tactic.PlanCampaignId;
                    _PlanTacticId = lstProcessedCustomFieldTactics[i].tactic.PlanTacticId;
                    _PlanProgramId = lstProcessedCustomFieldTactics[i].tactic.PlanProgramId;
                    _TypeId = IsCampaign ? tacticPlanCampaignId : (IsProgram ? _PlanProgramId : _PlanTacticId);

                    tacticListByViewById = lstTacticStageMap.Where(c => c.CustomFieldId == lstProcessedCustomFieldTactics[i].customFieldId).Select(c => c.CustomTacticList).FirstOrDefault();//lstTactic.Where(tactic => tacticItem.customFieldTacticList.Contains(IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId))).Select(tactic => tactic).ToList();
                    planminmaxdateobj = new PlanMinMaxDate();
                    planminmaxdateobj = planminmaxdatelist.Where(p => p.CustomFieldId == lstProcessedCustomFieldTactics[i].customFieldId && p.PlanId == tacticPlanId).FirstOrDefault();
                    #region "Get CampaignIds,ProgramIds & Tactic list for Min & Max Date"
                    fltrTactic = new List<Custom_Plan_Campaign_Program_Tactic>();
                    fltrTactic = tacticListByViewById.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.PlanProgramId : tactic.PlanTacticId)) == _TypeId).ToList();

                    #endregion
                    var tacProgramId = fltrTactic.Select(tac => tac.PlanProgramId).ToList();
                    var tacProgram = lstProgram.Where(prog => tacProgramId.Contains(prog.PlanProgramId)).ToList();
                    var tacCampId = fltrTactic.Select(tac => tac.PlanCampaignId).ToList();
                    var tacCamp = lstCampaign.Where(camp => tacCampId.Contains(camp.PlanCampaignId)).ToList();
                    #region "Get Tactic,Program & Campaign MaxDate"
                    maxDateTactic = fltrTactic.Select(tactic => tactic.EndDate).Max();
                    maxDateProgram = tacProgram.Select(prog => prog.EndDate).Max();
                    maxDateCampaign = tacCamp.Select(camp => camp.EndDate).Max();
                    MaxEndDateForCustomField = new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
                    #endregion
                    #region "Get Tactic,Program & Campaign MinDate"
                    minDateTactic = new DateTime();
                    minDateTactic = fltrTactic.Select(tactic => tactic.StartDate).Min();
                    minDateProgram = tacProgram.Select(prog => prog.StartDate).Min();
                    minDateCampaign = tacCamp.Select(camp => camp.StartDate).Min();
                    MinStartDateForCustomField = new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
                    #endregion


                    // Add By Nishant Sheth
                    // Desc :: To store Plan/Campaign/Program Progress becasue with same object not call progress function again #1798
                    #region Plan Progress
                    var PlanProgress = PlanProgresList.Where(plan => plan.EntityId == tacticPlanId).FirstOrDefault();
                    if (PlanProgress == null)
                    {
                        PlanProgresList.Add(new ProgressList
                        {
                            EntityId = tacticPlanId,
                            Progress = GetProgressPerformance(Common.GetStartDateAsPerCalendar(CalendarStartDate, planminmaxdateobj.minDate),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, planminmaxdateobj.minDate,
                                                    planminmaxdateobj.maxDate), tacticListByViewById, lstImprovementTactic, tacticPlanId)
                        });
                    }
                    PlanProgress = PlanProgresList.Where(plan => plan.EntityId == tacticPlanId).FirstOrDefault();
                    #endregion

                    #region Campaign Progress
                    var CampProgress = CampProgresList.Where(camp => camp.EntityId == lstProcessedCustomFieldTactics[i].tactic.PlanCampaignId).FirstOrDefault();
                    if (CampProgress == null)
                    {
                        CampProgresList.Add(new ProgressList
                        {
                            EntityId = lstProcessedCustomFieldTactics[i].tactic.PlanCampaignId,
                            Progress = GetCampaignProgressViewByPerformance(tacticListByViewById, lstCampaign.Where(a => a.PlanCampaignId == lstProcessedCustomFieldTactics[i].tactic.PlanCampaignId).FirstOrDefault(), planminmaxdateobj.minEffectiveDate)
                        });
                    }
                    CampProgress = CampProgresList.Where(camp => camp.EntityId == lstProcessedCustomFieldTactics[i].tactic.PlanCampaignId).FirstOrDefault();
                    #endregion

                    #region Program Progress
                    var ProgramProgress = ProgramProgresList.Where(prog => prog.EntityId == lstProcessedCustomFieldTactics[i].tactic.PlanProgramId).FirstOrDefault();
                    if (ProgramProgress == null)
                    {
                        ProgramProgresList.Add(new ProgressList
                        {
                            EntityId = lstProcessedCustomFieldTactics[i].tactic.PlanProgramId,
                            Progress = GetProgramProgressViewByPerformance(tacticListByViewById, lstProgram.Where(a => a.PlanProgramId == lstProcessedCustomFieldTactics[i].tactic.PlanProgramId).FirstOrDefault(), planminmaxdateobj.minEffectiveDate)
                        });
                    }
                    ProgramProgress = ProgramProgresList.Where(prog => prog.EntityId == lstProcessedCustomFieldTactics[i].tactic.PlanProgramId).FirstOrDefault();
                    #endregion
                    // End By Nishant Sheth

                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = Common.GetTacticFromCustomTacticModel(lstProcessedCustomFieldTactics[i].tactic),
                        //PlanTactic = lstProcessedCustomFieldTactics[i].tactic,
                        CustomTactic = lstProcessedCustomFieldTactics[i].tactic,
                        CustomFieldId = lstProcessedCustomFieldTactics[i].customFieldId.ToString(),
                        CustomFieldTitle = lstProcessedCustomFieldTactics[i].customFieldTitle,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", lstProcessedCustomFieldTactics[i].customFieldId, tacticPlanId, tacticPlanCampaignId, _PlanProgramId, _PlanTacticId),
                        ColorCode = TacticColor,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                        EndDate = Common.GetEndDateAsPerCalendarInDateFormat(CalendarEndDate, MaxEndDateForCustomField),
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField, MaxEndDateForCustomField),
                        CampaignProgress = CampProgress.Progress, // Modified By Nishant sheth #1798
                        ProgramProgress = ProgramProgress.Progress,// Modified By Nishant sheth #1798
                        PlanProgrss = PlanProgress.Progress,// Modified By Nishant sheth #1798
                        CreatedBy = lstProcessedCustomFieldTactics[i].CreatedBy,
                        PlanStartDate = planminmaxdateobj.minDate,
                        PlanEndDate = planminmaxdateobj.maxDate,
                        LinkTacticPermission = ((lstProcessedCustomFieldTactics[i].tactic.EndDate.Year - lstProcessedCustomFieldTactics[i].tactic.StartDate.Year) > 0) ? true : false,
                        LinkedTacticId = lstProcessedCustomFieldTactics[i].tactic.LinkedTacticId
                    });
                }
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
                PlanId = tacticTask.CustomTactic.PlanId,
                PlanTitle = tacticTask.CustomTactic.PlanTitle,
                Campaign = lstCampaign.Where(a => a.PlanCampaignId == tacticTask.CustomTactic.PlanCampaignId).FirstOrDefault(),
                Program = lstProgram.Where(a => a.PlanProgramId == tacticTask.CustomTactic.PlanProgramId).FirstOrDefault(),
                Tactic = tacticTask.Tactic,
                //PlanTactic = tacticTask.PlanTactic,
                CustomTactic = tacticTask.CustomTactic,
                PlanProgress = tacticTask.PlanProgrss,
                CampaignProgress = tacticTask.CampaignProgress,
                ProgramProgress = tacticTask.ProgramProgress,
                CreatedBy = tacticTask.CreatedBy,
                PlanStartDate = tacticTask.PlanStartDate,
                PlanEndDate = tacticTask.PlanEndDate,
                Status = tacticTask.CustomTactic.PlanStatus,
                LinkTacticPermission = tacticTask.LinkTacticPermission,
                LinkedTacticId = tacticTask.LinkedTacticId

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

            }).Distinct().OrderBy(tacticTask => tacticTask.text).ToList();

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
            #region Old Code
            // Commented By Nishant Sheth
            // Desc :: To resloved performance issue ticket #1798

            //var taskDataPlan = lstTaskDetails.Select(taskdata => new
            //{
            //    id = string.Format("Z{0}_L{1}", taskdata.MainParentId, taskdata.PlanId),
            //    text = taskdata.PlanTitle,
            //    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, viewBy == "Status" ? GetMinStartDateForPlanOfCustomField(viewBy, taskdata.MainParentId,
            //                taskdata.MainParentId, taskdata.PlanId, lstCampaign, lstProgram, lstTactic, IsCampaign, IsProgram) : taskdata.PlanStartDate),
            //    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
            //                                              viewBy == "Status" ? GetMinStartDateForPlanOfCustomField(viewBy, taskdata.MainParentId,
            //                                              taskdata.MainParentId, taskdata.PlanId, lstCampaign, lstProgram,
            //                                              lstTactic, IsCampaign, IsProgram) : taskdata.PlanStartDate,
            //                                              viewBy == "Status" ? GetMaxEndDateForPlanOfCustomFields(viewBy, taskdata.MainParentId,
            //                                              taskdata.MainParentId, taskdata.PlanId, lstCampaign, lstProgram,
            //                                              lstTactic, IsCampaign, IsProgram) : taskdata.PlanEndDate),
            //    progress = taskdata.PlanProgress,
            //    open = false,
            //    parent = string.Format("Z{0}", taskdata.MainParentId),
            //    color = PlanColor,
            //    planid = taskdata.PlanId,
            //    CreatedBy = taskdata.CreatedBy,
            //    Status = taskdata.Status  //added by Rahul Shah on 16/12/2015 for PL #1782

            //}).Select(taskdata => taskdata).Distinct().OrderBy(taskdata => taskdata.text).ToList();
            #endregion

            // Add By Nishant Sheth
            // Desc :: To improve performance with change view by option ticket #1798
            List<TaskDataPlan> ListTaskDataPlan = new List<TaskDataPlan>();
            List<StartMin_EndMax_Plan> StartMin_EndMax_Plan = new List<StartMin_EndMax_Plan>(); // Get list of Plan Minimum Start Date & Max End Date
            List<StartMin_Duration_Plan> StartMin_Duration_Plan = new List<StartMin_Duration_Plan>();// Get list of Plan Start Date & Duration
            if (viewBy == "Status")
            {

                for (int i = 0; i < lstTaskDetails.Count; i++)
                {
                    string id = string.Format("Z{0}_L{1}", lstTaskDetails[i].MainParentId, lstTaskDetails[i].PlanId);
                    string parent = string.Format("Z{0}", lstTaskDetails[i].MainParentId);

                    var StartMin_EndMax_Date = StartMin_EndMax_Plan.Where(a => a.ParentId == parent && a.PlanId == id).FirstOrDefault();
                    if (StartMin_EndMax_Date == null)
                    {
                        var MinStartDate = GetMinStartDateForPlanOfCustomField(viewBy, lstTaskDetails[i].MainParentId,
                                lstTaskDetails[i].MainParentId, lstTaskDetails[i].PlanId, lstCampaign, lstProgram, lstTactic, IsCampaign, IsProgram);
                        var MaxEndDate = GetMaxEndDateForPlanOfCustomFields(viewBy, lstTaskDetails[i].MainParentId,
                                                              lstTaskDetails[i].MainParentId, lstTaskDetails[i].PlanId, lstCampaign, lstProgram,
                                                              lstTactic, IsCampaign, IsProgram);
                        StartMin_EndMax_Plan.Add(new StartMin_EndMax_Plan { ParentId = parent, PlanId = id, MinStartDate = MinStartDate, MaxEndDate = MaxEndDate });
                    }
                    StartMin_EndMax_Date = StartMin_EndMax_Plan.Where(a => a.ParentId == parent && a.PlanId == id).FirstOrDefault();

                    var startDuration = StartMin_Duration_Plan.Where(a => a.ParentId == parent && a.PlanId == id).FirstOrDefault();
                    if (startDuration == null)
                    {
                        var MinStartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, StartMin_EndMax_Date.MinStartDate);
                        var Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, StartMin_EndMax_Date.MinStartDate, StartMin_EndMax_Date.MaxEndDate);
                        StartMin_Duration_Plan.Add(new StartMin_Duration_Plan { PlanId = id, ParentId = parent, MinStartDate = MinStartDate, Duration = Duration });
                    }
                    startDuration = StartMin_Duration_Plan.Where(a => a.ParentId == parent && a.PlanId == id).FirstOrDefault();
                    ListTaskDataPlan.Add(new TaskDataPlan
                    {
                        id = id,
                        text = lstTaskDetails[i].PlanTitle,
                        start_date = startDuration.MinStartDate,
                        duration = startDuration.Duration,
                        progress = lstTaskDetails[i].PlanProgress,
                        open = false,
                        parent = parent,
                        color = PlanColor,
                        planid = lstTaskDetails[i].PlanId,
                        CreatedBy = lstTaskDetails[i].CreatedBy,
                        Status = lstTaskDetails[i].Status  //added by Rahul Shah on 16/12/2015 for PL #1782
                    });

                }

            }
            else
            {
                for (int i = 0; i < lstTaskDetails.Count; i++)
                {
                    string id = string.Format("Z{0}_L{1}", lstTaskDetails[i].MainParentId, lstTaskDetails[i].PlanId);
                    string parent = string.Format("Z{0}", lstTaskDetails[i].MainParentId);
                    var StartMin_EndMax_Date = StartMin_EndMax_Plan.Where(a => a.ParentId == parent && a.PlanId == id).FirstOrDefault();
                    if (StartMin_EndMax_Date == null)
                    {
                        var MinStartDate = lstTaskDetails[i].PlanStartDate;
                        var MaxEndDate = lstTaskDetails[i].PlanEndDate;
                        StartMin_EndMax_Plan.Add(new StartMin_EndMax_Plan { ParentId = parent, PlanId = id, MinStartDate = MinStartDate, MaxEndDate = MaxEndDate });
                    }
                    StartMin_EndMax_Date = StartMin_EndMax_Plan.Where(a => a.ParentId == parent && a.PlanId == id).FirstOrDefault();

                    var startDuration = StartMin_Duration_Plan.Where(a => a.ParentId == parent && a.PlanId == id).FirstOrDefault();
                    if (startDuration == null)
                    {
                        var MinStartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, StartMin_EndMax_Date.MinStartDate);
                        var Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, StartMin_EndMax_Date.MinStartDate, StartMin_EndMax_Date.MaxEndDate);
                        StartMin_Duration_Plan.Add(new StartMin_Duration_Plan { PlanId = id, ParentId = parent, MinStartDate = MinStartDate, Duration = Duration });
                    }
                    startDuration = StartMin_Duration_Plan.Where(a => a.ParentId == parent && a.PlanId == id).FirstOrDefault();
                    ListTaskDataPlan.Add(new TaskDataPlan
                    {
                        id = id,
                        text = lstTaskDetails[i].PlanTitle,
                        start_date = startDuration.MinStartDate,
                        duration = startDuration.Duration,
                        progress = lstTaskDetails[i].PlanProgress,
                        open = false,
                        parent = parent,
                        color = PlanColor,
                        planid = lstTaskDetails[i].PlanId,
                        CreatedBy = lstTaskDetails[i].CreatedBy,
                        Status = lstTaskDetails[i].Status  //added by Rahul Shah on 16/12/2015 for PL #1782
                    });
                }
            }

            var taskDataPlan = ListTaskDataPlan.Select(taskdata => new
            {
                id = taskdata.id,
                text = taskdata.text,
                start_date = taskdata.start_date,
                duration = taskdata.duration,
                progress = taskdata.progress,
                open = taskdata.open,
                parent = taskdata.parent,
                color = taskdata.color,
                planid = taskdata.planid,
                CreatedBy = taskdata.CreatedBy,
                Status = taskdata.Status
            }).Distinct().OrderBy(taskdata => taskdata.text).ToList();
            // End By Nishant Sheth


            //// Finalize Plan task data to be render in gantt chart
            var lstPlanTaskdata = taskDataPlan.Select(taskdata => new
            {
                id = taskdata.id,
                text = taskdata.text,
                machineName = "",
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
                OwnerName = GetOwnerName(taskdata.CreatedBy),
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
                machineName = "",
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
                OwnerName = GetOwnerName(taskdata.CreatedBy),
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
                machineName = "",
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
                OwnerName = GetOwnerName(taskdata.CreatedBy),
                Permission = IsPlanCreateAllAuthorized == false ? (taskdata.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(taskdata.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
            }).ToList().Distinct().ToList();
            #endregion

            #region Prepare Tactic task data
            //Added by Komal Rawal for #1845 Link tactic to plan 

            var LinkedTacticList = lstTactic.Where(list => list.LinkedTacticId != null && list.LinkedPlanId != null).ToList();

            var ListOfLinkedPlanIds = LinkedTacticList.Select(list =>
                      list.LinkedPlanId
               ).ToList();

            // Commented by nishant sheth
            // Desc :: to avoid db trip for linked plan tactic's plan name
            // Modidfied By Komal Rawal for wrong data

            Dictionary<int, string> PlanNames = new Dictionary<int, string>();
            objDbMrpEntities.Plans.Where(Id => ListOfLinkedPlanIds.Contains(Id.PlanId)).Select(list => list).ToList().ForEach(p => PlanNames.Add(p.PlanId, p.Title));

            var ListOfLinkedTactics = LinkedTacticList.Select(list =>
                    new
                    {
                        TacticId = list.LinkedTacticId,
                        PlanName = list.LinkedPlanId == null ? null : PlanNames[(int)list.LinkedPlanId]
                    }
                );
            //End
            List<int> _PlanIds = lstTaskDetails.Select(_task => _task.PlanId).Distinct().ToList();
            List<ProgressModel> _EffectiveDateListByPlanIds = lstImprovementTactic.Where(imprvmnt => _PlanIds.Contains(imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).Select(imprvmnt => new ProgressModel { PlanId = imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, EffectiveDate = imprvmnt.EffectiveDate }).ToList();
            var taskDataTactic = lstTaskDetails.Select(taskdata => new
            {
                id = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", taskdata.MainParentId, taskdata.PlanId, taskdata.Program.PlanCampaignId, taskdata.Program.PlanProgramId, taskdata.Tactic.PlanTacticId),
                text = taskdata.Tactic.Title,
                machineName = taskdata.Tactic.TacticCustomName,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, taskdata.Tactic.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, taskdata.Tactic.StartDate, taskdata.Tactic.EndDate),
                progress = GetTacticProgress((taskdata.Tactic.StartDate != null ? taskdata.Tactic.StartDate : new DateTime()), _EffectiveDateListByPlanIds, taskdata.PlanId),
                open = false,
                parent = string.Format("Z{0}_L{1}_C{2}_P{3}", taskdata.MainParentId, taskdata.PlanId, taskdata.Program.PlanCampaignId, taskdata.Tactic.PlanProgramId),
                color = taskdata.Color,
                plantacticid = taskdata.Tactic.PlanTacticId,
                Status = taskdata.Tactic.Status,
                TacticTypeId = taskdata.Tactic.TacticTypeId,
                CreatedBy = taskdata.CreatedBy,
                LinkTacticPermission = taskdata.LinkTacticPermission,
                LinkedTacticId = taskdata.LinkedTacticId,
                LinkedPlanName = ListOfLinkedTactics.Where(id => id.TacticId.Equals(taskdata.LinkedTacticId)).Select(a => a.PlanName).FirstOrDefault()

            }).Distinct().OrderBy(t => t.text);

            //// Finalize Tactic task data to be render in gantt chart
            var lstTacticTaskData = taskDataTactic.Select(taskdata => new
            {
                id = taskdata.id,
                text = taskdata.text,
                machineName = taskdata.machineName,
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
                OwnerName = GetOwnerName(taskdata.CreatedBy),
                Permission = IsPlanCreateAllAuthorized == false ? (taskdata.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(taskdata.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized,
                LinkTacticPermission = taskdata.LinkTacticPermission,
                LinkedTacticId = taskdata.LinkedTacticId,
                LinkedPlanName = taskdata.LinkedPlanName
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
                machineName = "",
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
        private JsonResult PrepareTacticAndRequestTabResult(List<int> filterplanId, string viewBy, bool IsFiltered, bool IsRequest, Enums.ActiveMenu activemenu, List<Plan_Campaign> lstCampaign, List<Custom_Plan_Campaign_Program> lstProgram, List<Custom_Plan_Campaign_Program_Tactic> lstTactic, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion, List<ViewByModel> viewByListResult, List<Guid> filterOwner, List<string> filterStatus, List<Plan> lstPlans, string timeframe = "")
        {
            Dictionary<string, string> ColorCodelist = objDbMrpEntities.EntityTypeColors.ToDictionary(e => e.EntityType.ToLower(), e => e.ColorCode);
            List<object> tacticAndRequestTaskData = GetTaskDetailTactic(ColorCodelist, filterplanId, viewBy, IsFiltered, IsRequest, planYear, activemenu, lstCampaign.ToList(), lstProgram.ToList(), lstTactic.ToList(), lstImprovementTactic, filterOwner, filterStatus, lstPlans, timeframe); // Modified By Nishant #1915
            //   Debug.WriteLine("Step 7.1: " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            //Added By komal Rawal for #1282

            var TacticColor = ColorCodelist[Enums.EntityType.Tactic.ToString().ToLower()];


            if (activemenu.Equals(Enums.ActiveMenu.Home))
            {
                //// Prepate tactic list for Marketting Acticities accordian
                var planCampaignProgramTactic = lstTactic.ToList().Select(tactic => new
                {
                    PlanTacticId = tactic.PlanTacticId,
                    TacticTypeId = tactic.TacticTypeId,
                    Title = tactic.Title,
                    TaskId = string.Format("L{0}_C{1}_P{2}_T{3}_Y{4}", tactic.PlanId, tactic.PlanCampaignId, tactic.PlanProgramId, tactic.PlanTacticId, tactic.TacticTypeId)
                });

                //// Prepate tactic type list for Marketting Acticities accordian
                var tacticType = lstTactic.Select(tactic => new
                {
                    tactic.TacticTypeId,
                    tactic.TacticTypeTtile,
                    ColorCode = TacticColor
                }).Distinct().OrderBy(tactic => tactic.TacticTypeTtile);

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
        public List<object> GetTaskDetailTactic(Dictionary<string, string> ColorCodelist, List<int> filterplanId, string viewBy, bool IsFiltered, bool IsRequest, string planYear, Enums.ActiveMenu activemenu, List<Plan_Campaign> lstCampaign, List<Custom_Plan_Campaign_Program> lstProgram, List<Custom_Plan_Campaign_Program_Tactic> lstTactic, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic, List<Guid> filterOwner, List<string> filterStatus, List<Plan> lstPlans, string timeframe = "")
        {

            //DateTime StartDate = DateTime.Now.Date;
            //DateTime EndDate = DateTime.Now.Date;
            //StartDate = new DateTime(Convert.ToInt32(planYear), 1, 1);
            //EndDate = new DateTime(Convert.ToInt32(planYear) + 1, 1, 1).AddTicks(-1);
            // Add By Nishant
            Common.GetPlanGanttStartEndDate(planYear, timeframe, ref CalendarStartDate, ref CalendarEndDate);


            string[] listYear = timeframe.Split('-'); //PL #1960 Dashrath Prajapati


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
                tacticStageRelationList = Common.GetTacticStageRelationPerformance(lstTactic.Select(tactic => tactic).ToList(), false);
                stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).ToList();
                inqLevel = Convert.ToInt32(stageList.FirstOrDefault(stage => stage.Code == inqstage).Level);
                mqlLevel = Convert.ToInt32(stageList.FirstOrDefault(stage => stage.Code == mqlstage).Level);
            }

            // Add By Nishant Sheth
            // DESC:: For get default filter view after user log out #1750
            //var Label = Enums.FilterLabel.Plan.ToString();
            //var SetOfPlanSelected = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.FilterName != Label && view.Userid == Sessions.User.UserId && view.ViewName == null).Select(View => View.FilterValues).ToList();
            //Added by Komal Rawal for #1845 Link tactic to plan 

            var LinkedTacticList = lstTactic.Where(list => list.LinkedTacticId != null && list.LinkedPlanId != null).ToList();

            var ListOfLinkedPlanIds = LinkedTacticList.Select(list =>
                      list.LinkedPlanId
               ).Distinct().ToList();

            var ListOfLinkedPlans = objDbMrpEntities.Plans.Where(Id => ListOfLinkedPlanIds.Contains(Id.PlanId)).Select(list => list).ToList();

            Dictionary<int, string> PlanNames = new Dictionary<int, string>();
            objDbMrpEntities.Plans.Where(Id => ListOfLinkedPlanIds.Contains(Id.PlanId)).Select(list => list).ToList().ForEach(p => PlanNames.Add(p.PlanId, p.Title));

            var ListOfLinkedTactics = LinkedTacticList.Select(list =>
                    new
                    {
                        TacticId = list.LinkedTacticId,
                        PlanName = list.LinkedPlanId == null ? null : PlanNames[(int)list.LinkedPlanId]
                    }
                );
            //End

            if (IsRequest) //When clicked on request tab data will be displayed in bottom up approach else top-down for ViewBy Tactic
            {

                #region Prepare Plan Task Data
                //// Prepare task data plan list for gantt chart

                //var planall = lstTactic.Select(tactic => tactic.objPlanTacticCampaignPlan).Distinct().ToList();
                var planall = lstPlans.ToList();

                var taskDataPlan = planall.Select(objplan => new
                {
                    id = string.Format("L{0}", objplan.PlanId),
                    text = objplan.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Tactic, objplan.PlanId, lstCampaign, lstProgram, lstTactic)),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                              GetMinStartDateForPlan(GanttTabs.Tactic, objplan.PlanId, lstCampaign, lstProgram, lstTactic),
                                                              GetMaxEndDateForPlan(GanttTabs.Tactic, objplan.PlanId, lstCampaign, lstProgram, lstTactic)),
                    progress = GetProgressPerformance(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Tactic, objplan.PlanId, lstCampaign, lstProgram, lstTactic)),
                                                        Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                                  GetMinStartDateForPlan(GanttTabs.Tactic, objplan.PlanId, lstCampaign, lstProgram, lstTactic),
                                                                  GetMaxEndDateForPlan(GanttTabs.Tactic, objplan.PlanId, lstCampaign, lstProgram, lstTactic)),
                                                       lstTactic, lstImprovementTactic, objplan.PlanId),
                    open = false,
                    color = PlanColor,
                    planid = objplan.PlanId,
                    CreatedBy = objplan.CreatedBy,
                    Status = objplan.Status
                }).Select(objplan => objplan).OrderBy(objplan => objplan.text).ToList();



                //// Finalize task data plan list for gantt chart
                var newTaskDataPlan = taskDataPlan.Select(plan => new
                {
                    id = plan.id,
                    text = plan.text,
                    machineName = "",
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
                    OwnerName = GetOwnerName(plan.CreatedBy),
                    Permission = IsPlanCreateAllAuthorized == false ? (plan.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(plan.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                }).ToList();
                #endregion

                //// Get list of plan Ids
                var planIdList = lstTactic.Select(tactic => tactic.PlanId).ToList().Distinct();


                #region Prepare Plan data for Improvement
                //// Prepate task data improvement Tacic list for gantt chart
                var taskDataPlanForImprovement = lstImprovementTactic.Where(improvementTactic => !planIdList.Contains(improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId) && improvementTactic.EffectiveDate <= CalendarEndDate && improvementTactic.EffectiveDate >= CalendarStartDate).Select(improvementTactic => new
                {
                    id = string.Format("L{0}", improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                    text = improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.None, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic)),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                              GetMinStartDateForPlan(GanttTabs.None, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic),
                                                              GetMaxEndDateForPlan(GanttTabs.None, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic)),
                    progress = GetProgressPerformance(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.None, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic)),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                              GetMinStartDateForPlan(GanttTabs.None, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic),
                                                              GetMaxEndDateForPlan(GanttTabs.None, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic)),
                                                   lstTactic, lstImprovementTactic, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                    open = false,
                    color = PlanColor,
                    planid = improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId,
                    CreatedBy = improvementTactic.CreatedBy

                }).Select(improvementTactic => improvementTactic).Distinct().OrderBy(improvementTactic => improvementTactic.text).ToList();

                //// Finalize task data Improvement Tactic list for gantt chart
                var newTaskDataPlanForImprovement = taskDataPlanForImprovement.Select(improvementTactic => new
                {
                    id = improvementTactic.id,
                    text = improvementTactic.text,
                    machineName = "",
                    start_date = improvementTactic.start_date,
                    duration = improvementTactic.duration,
                    progress = improvementTactic.progress,
                    open = improvementTactic.open,
                    color = (improvementTactic.progress > 0 ? "stripe" : string.Empty),
                    colorcode = improvementTactic.color,
                    planid = improvementTactic.planid,
                    type = "Plan",
                    Permission = IsPlanCreateAllAuthorized == false ? (improvementTactic.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(improvementTactic.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                }).ToList();
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
                    machineName = "",
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
                List<int> PlanIds = lstTactic.Select(_tac => int.Parse(_tac.PlanId.ToString())).ToList();
                List<ProgressModel> EffectiveDateListByPlanIds = lstImprovementTactic.Where(imprvmnt => PlanIds.Contains(imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).Select(imprvmnt => new ProgressModel { PlanId = imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, EffectiveDate = imprvmnt.EffectiveDate }).ToList();

                //// Prepare task data tactic list for gantt chart
                var taskDataTactic = lstTactic.Select(tactic => new
                {
                    id = string.Format("L{0}_C{1}_P{2}_T{3}_Y{4}", tactic.PlanId, tactic.PlanCampaignId, tactic.PlanProgramId, tactic.PlanTacticId, tactic.TacticTypeId),
                    text = tactic.Title,
                    machineName = tactic.TacticCustomName,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.StartDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, tactic.StartDate, tactic.EndDate),
                    progress = GetTacticProgress((tactic.StartDate != null ? tactic.StartDate : new DateTime()), EffectiveDateListByPlanIds, tactic.PlanId),
                    open = false,
                    parent = string.Format("L{0}_C{1}_P{2}", tactic.PlanId, tactic.PlanCampaignId, tactic.PlanProgramId),
                    color = TacticColor,
                    isSubmitted = tactic.Status == tacticStatusSubmitted,
                    isDeclined = tactic.Status == tacticStatusDeclined,
                    projectedStageValue = viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase) ? stageList.FirstOrDefault(s => s.StageId == tactic.StageId).Level <= inqLevel ? Convert.ToString(tacticStageRelationList.FirstOrDefault(tm => tm.TacticObj.PlanTacticId == tactic.PlanTacticId).INQValue) : "N/A" : "0",
                    mqls = viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase) ? stageList.FirstOrDefault(s => s.StageId == tactic.StageId).Level <= mqlLevel ? Convert.ToString(tacticStageRelationList.FirstOrDefault(tacticStage => tacticStage.TacticObj.PlanTacticId == tactic.PlanTacticId).MQLValue) : "N/A" : "0",
                    cost = tactic.Cost,
                    cws = viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase) ? tactic.Status == tacticStatusSubmitted || tactic.Status == tacticStatusDeclined ? Math.Round(tacticStageRelationList.FirstOrDefault(tacticStage => tacticStage.TacticObj.PlanTacticId == tactic.PlanTacticId).RevenueValue, 1) : 0 : 0,
                    plantacticid = tactic.PlanTacticId,
                    Status = tactic.Status,
                    TacticTypeId = tactic.TacticTypeId,
                    CreatedBy = tactic.CreatedBy,
                    LinkTacticPermission = ((tactic.EndDate.Year - tactic.StartDate.Year) > 0) ? true : false,
                    LinkedTacticId = tactic.LinkedTacticId,
                    LinkedPlanName = ListOfLinkedTactics.Where(id => id.TacticId.Equals(tactic.LinkedTacticId)).Select(a => a.PlanName).FirstOrDefault()

                }).OrderBy(tactic => tactic.text);

                //// Finalize task data tactic list for gantt chart
                var NewTaskDataTactic = taskDataTactic.Select(tactic => new
                {
                    id = tactic.id,
                    text = tactic.text,
                    machineName = tactic.machineName,
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
                    OwnerName = GetOwnerName(tactic.CreatedBy),
                    Permission = IsPlanCreateAllAuthorized == false ? (tactic.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(tactic.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized,
                    LinkTacticPermission = tactic.LinkTacticPermission,
                    LinkedTacticId = tactic.LinkedTacticId,
                    LinkedPlanName = tactic.LinkedPlanName
                });
                #endregion

                #region Prepare Program Task Data
                //// Prepare task data program list for gantt chart
                var taskDataProgram = lstTactic.Select(tactic => new
                {
                    id = string.Format("L{0}_C{1}_P{2}", tactic.PlanId, tactic.PlanCampaignId, tactic.PlanProgramId),
                    text = tactic.ProgramTitle,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.StartDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, tactic.StartDate, tactic.EndDate),
                    //progress = GetProgramProgress(lstTactic, tactic.objPlanTacticProgram, EffectiveDateListByPlanIds, tactic.PlanId),
                    progress = GetProgramProgress(lstTactic, lstProgram.Where(prg => prg.PlanProgramId == tactic.PlanProgramId).FirstOrDefault(), EffectiveDateListByPlanIds, tactic.PlanId),
                    open = false,
                    parent = string.Format("L{0}_C{1}", tactic.PlanId, tactic.PlanCampaignId),
                    color = ProgramColor,
                    planprogramid = tactic.PlanProgramId,
                    Status = tactic.Status,
                    CreatedBy = tactic.CreatedBy

                }).Select(tactic => tactic).Distinct().OrderBy(tactic => tactic.text);

                //// Finalize task data program list for gantt chart
                var newTaskDataProgram = taskDataProgram.Select(program => new
                {
                    id = program.id,
                    text = program.text,
                    machineName = "",
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
                    OwnerName = GetOwnerName(program.CreatedBy),
                    Permission = IsPlanCreateAllAuthorized == false ? (program.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(program.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });
                #endregion

                #region Prepare Campaign Task Data
                //// Prepare task data campaign list for gantt chart
                var taskDataCampaign = lstTactic.Select(tactic => new
                {
                    id = string.Format("L{0}_C{1}", tactic.PlanId, tactic.PlanCampaignId),
                    text = tactic.CampaignTitle,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.StartDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, tactic.StartDate, tactic.EndDate),
                    progress = GetCampaignProgress(lstTactic, lstCampaign.Where(camp => camp.PlanCampaignId == tactic.PlanCampaignId).FirstOrDefault(), EffectiveDateListByPlanIds, tactic.PlanId),
                    open = false,
                    parent = string.Format("L{0}", tactic.PlanId),
                    color = CampaignColor,
                    plancampaignid = tactic.PlanCampaignId,
                    Status = tactic.Status,
                    CreatedBy = tactic.CreatedBy
                }).Select(tactic => tactic).Distinct().OrderBy(tactic => tactic.text);

                //// Finalize task data campaign list for gantt chart
                var newTaskDataCampaign = taskDataCampaign.Select(campaign => new
                {
                    id = campaign.id,
                    text = campaign.text,
                    machineName = "",
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
                    OwnerName = GetOwnerName(campaign.CreatedBy),
                    Permission = IsPlanCreateAllAuthorized == false ? (campaign.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(campaign.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });
                #endregion
                taskDataPlanMerged = taskDataPlanMerged.Concat<object>(newTaskDataCampaign).Concat<object>(NewTaskDataTactic).Concat<object>(newTaskDataProgram);
                return taskDataPlanMerged.ToList<object>();

            }
            else
            {
                //Get list of ids for entity if filter is apply on tactic other than owner & status
                List<int> PlanIds = lstTactic.Select(_tac => _tac.PlanId).ToList();
                List<int> OwnerFilterCampaignIds = lstTactic.Select(_tac => _tac.PlanCampaignId).ToList();
                List<int> OwnerFilterProgramids = lstTactic.Select(_tac => _tac.PlanProgramId).ToList();
                List<int> OwnerFilterTacticIds = lstTactic.Select(_tac => _tac.PlanTacticId).ToList();
                List<ProgressModel> EffectiveDateListByPlanIds = lstImprovementTactic.Where(imprvmnt => PlanIds.Contains(imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).Select(imprvmnt => new ProgressModel { PlanId = imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, EffectiveDate = imprvmnt.EffectiveDate }).ToList();

                #region Prepare Tactic data for Plan
                //// Prepare task data tactic list for gantt chart

                var taskDataTacticforPlanMain = lstTactic.Where(_tac => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                            CalendarEndDate,
                                                                                                                            _tac.StartDate,
                                                                                                                            _tac.EndDate).Equals(false) && (((filterOwner.Count > 0 ? filterOwner.Contains(_tac.CreatedBy) : true) && (filterStatus.Count > 0 ? filterStatus.Contains(_tac.Status) : true)) || OwnerFilterTacticIds.Contains(_tac.PlanTacticId))).ToList();
                var taskDataTacticforPlan = taskDataTacticforPlanMain.Select(_tac => new
                {
                    id = string.Format("L{0}_C{1}_P{2}_T{3}_Y{4}", _tac.PlanId, _tac.PlanCampaignId, _tac.PlanProgramId, _tac.PlanTacticId, _tac.TacticTypeId),
                    text = _tac.Title,
                    machineName = _tac.TacticCustomName,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, _tac.StartDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                               CalendarEndDate,
                                                                 _tac.StartDate,
                                                                 _tac.EndDate),
                    //   progress = GetTacticProgress( _tac, ImprovementTacticForTaskData),
                    progress = GetTacticProgress((_tac.StartDate != null ? _tac.StartDate : new DateTime()), EffectiveDateListByPlanIds, _tac.PlanId),
                    // progress = 0,
                    open = false,
                    isSubmitted = _tac.Status == tacticStatusSubmitted,
                    isDeclined = _tac.Status == tacticStatusDeclined,
                    projectedStageValue = viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase) ? stageList.FirstOrDefault(s => s.StageId == _tac.StageId).Level <= inqLevel ? Convert.ToString(tacticStageRelationList.FirstOrDefault(tm => tm.TacticObj.PlanTacticId == _tac.PlanTacticId).INQValue) : "N/A" : "0",
                    mqls = viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase) ? stageList.FirstOrDefault(s => s.StageId == _tac.StageId).Level <= mqlLevel ? Convert.ToString(tacticStageRelationList.FirstOrDefault(tacticStage => tacticStage.TacticObj.PlanTacticId == _tac.PlanTacticId).MQLValue) : "N/A" : "0",
                    cost = _tac.Cost,
                    cws = viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase) ? _tac.Status == tacticStatusSubmitted || _tac.Status == tacticStatusDeclined ? Math.Round(tacticStageRelationList.FirstOrDefault(tacticStage => tacticStage.TacticObj.PlanTacticId == _tac.PlanTacticId).RevenueValue, 1) : 0 : 0,
                    parent = string.Format("L{0}_C{1}_P{2}", _tac.PlanId, _tac.PlanCampaignId, _tac.PlanProgramId),
                    color = TacticColor,
                    plantacticid = _tac.PlanTacticId,
                    Status = _tac.Status,
                    TacticTypeId = _tac.TacticTypeId,
                    CreatedBy = _tac.CreatedBy,
                    LinkTacticPermission = ((_tac.EndDate.Year - _tac.StartDate.Year) > 0) ? true : false,
                    LinkedTacticId = _tac.LinkedTacticId,
                    LinkedPlanName = ListOfLinkedTactics.Where(id => id.TacticId.Equals(_tac.LinkedTacticId)).Select(a => a.PlanName).FirstOrDefault()

                }).OrderBy(_tac => _tac.text).ToList();

                List<int> lstAllowedEntityIds = new List<int>();
                if (lstTactic.Count() > 0)
                {
                    lstAllowedEntityIds = lstTactic.Select(tactic => tactic.PlanTacticId).Distinct().ToList();
                }

                var NewTaskDataTacticforPlan = taskDataTacticforPlan.Where(task => !lstAllowedEntityIds.Count.Equals(0) && (lstAllowedEntityIds.Contains(task.plantacticid))).Select(task => new
                {
                    id = task.id,
                    text = task.text,
                    machineName = task.machineName,
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
                    OwnerName = GetOwnerName(task.CreatedBy),
                    Permission = IsPlanCreateAllAuthorized == false ? (task.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(task.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized,
                    LinkTacticPermission = task.LinkTacticPermission,
                    LinkedTacticId = task.LinkedTacticId,
                    LinkedPlanName = task.LinkedPlanName

                });
                #endregion

                List<int> OwnerFilterProgramidsMain = OwnerFilterProgramids.Concat(taskDataTacticforPlanMain.Select(t => t.PlanProgramId).ToList()).ToList();

                #region Prepare Program Task Data for Plan
                //Modified by Maitri Gandhi for #2037, on 4/3/2016
                var taskDataProgramforPlanMain = lstProgram.Where(prgrm => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                    CalendarEndDate,
                                                                                                                    prgrm.StartDate,
                                                                                                                    prgrm.EndDate).Equals(false)).ToList(); //&& (((filterOwner.Count > 0 ? filterOwner.Contains(prgrm.CreatedBy) : true) && (filterStatus.Count > 0 ? filterStatus.Contains(prgrm.Status) : true)) || OwnerFilterProgramidsMain.Contains(prgrm.PlanProgramId))
                var taskDataProgramforPlan = taskDataProgramforPlanMain.Select(prgrm => new
                {
                    id = string.Format("L{0}_C{1}_P{2}", prgrm.PlanId, prgrm.PlanCampaignId, prgrm.PlanProgramId),
                    text = prgrm.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, prgrm.StartDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                              CalendarEndDate,
                                                              prgrm.StartDate,
                                                              prgrm.EndDate),
                    // progress = GetProgramProgress( prgrm, ImprovementTacticForTaskData),
                    progress = GetProgramProgress(lstTactic, prgrm, EffectiveDateListByPlanIds, prgrm.PlanId),
                    // progress = 0,
                    open = false,
                    parent = string.Format("L{0}_C{1}", prgrm.PlanId, prgrm.PlanCampaignId),
                    color = ProgramColor,
                    planprogramid = prgrm.PlanProgramId,
                    Status = prgrm.Status,
                    CreatedBy = prgrm.CreatedBy
                }).Select(prgrm => prgrm).Distinct().OrderBy(prgrm => prgrm.text).ToList();

                var NewtaskDataProgramforPlan = taskDataProgramforPlan.Select(task => new
                {
                    id = task.id,
                    text = task.text,
                    machineName = "",
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
                    OwnerName = GetOwnerName(task.CreatedBy),
                    Permission = IsPlanCreateAllAuthorized == false ? (task.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(task.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });
                #endregion

                List<int> OwnerFilterCampaignidsMain = OwnerFilterCampaignIds.Concat(taskDataProgramforPlanMain.Select(p => p.PlanCampaignId).ToList()).ToList();

                #region Prepare Campaign Task Data for PLan
                //Modified by Maitri Gandhi for #2037, on 4/3/2016
                var taskDataCampaignforPlanMain = lstCampaign.Where(_campgn => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                CalendarEndDate,
                                                                                                                _campgn.StartDate,
                                                                                                                _campgn.EndDate).Equals(false)).ToList();// && (((filterOwner.Count > 0 ? filterOwner.Contains(_campgn.CreatedBy) : true) && (filterStatus.Count > 0 ? filterStatus.Contains(_campgn.Status) : true)) || OwnerFilterCampaignidsMain.Contains(_campgn.PlanCampaignId))
                var taskDataCampaignforPlan = taskDataCampaignforPlanMain.Select(_campgn => new
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
                    machineName = "",
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
                    OwnerName = GetOwnerName(_campgn.CreatedBy),
                    Permission = IsPlanCreateAllAuthorized == false ? (_campgn.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(_campgn.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });

                #endregion

                List<int> OwnerFilterPlanidsMain = PlanIds.Concat(taskDataCampaignforPlanMain.Select(c => c.PlanId).ToList()).ToList();

                #region Improvement Activities & Tactics
                //// Prepare list of Improvement Tactic and Activities list for gantt chart
                // var improvemntTacticList1 = lstImprovementTactic.Where(improvementTactic => improvementTactic.EffectiveDate > CalendarEndDate).ToList();
                var improvemntTacticList = lstImprovementTactic.Where(improvementTactic => improvementTactic.EffectiveDate <= CalendarEndDate && improvementTactic.EffectiveDate >= CalendarStartDate).Select(improvementTactic => new
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
                    machineName = "",
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
                    machineName = "",
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

                #region Plan
                //Modified by Komal Rawal for #1537 to get Plan according to year.
                //                List<int> campplanid = new List<int>();
                // Change by Nishant Sheth for remove double db trip.
                //var planData = objDbMrpEntities.Plans.Where(plan => filterplanId.Contains(plan.PlanId) && plan.IsDeleted.Equals(false)).Select(a => a).ToList();
                // Add By Nishant Sheth
                // Desc :: get data from cache records
                var planData = ((List<Plan>)objCache.Returncache(Enums.CacheObject.Plan.ToString())).ToList();
                var planList = planData.Where(plan => filterplanId.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && listYear.Contains(plan.Year)).Select(a => a.PlanId).ToList(); ////PL #1960 Dashrath Prajapati
                List<int> campplanid = new List<int>();
                List<int> campplanid1 = new List<int>();
                if (planList.Count == 0)
                {
                    //campplanid = objDbMrpEntities.Plan_Campaign.Where(camp => !(camp.StartDate > CalendarEndDate || camp.EndDate < CalendarStartDate) && filterplanId.Contains(camp.PlanId)).Select(a => a.PlanId).Distinct().ToList();
                    // Add By Nishant Sheth
                    // Desc :: get data from cache records
                    campplanid = ((List<Plan_Campaign>)objCache.Returncache(Enums.CacheObject.Campaign.ToString())).Where(camp => !(camp.StartDate > CalendarEndDate || camp.EndDate < CalendarStartDate)).Select(a => a.PlanId).Distinct().ToList();

                }
                //var cmgnidlist = taskDataCampaignforPlanMain.Select(i => i.Plan.PlanId); //PL #1960 Dashrath Prajapati
                var cmgnidlist = taskDataCampaignforPlanMain.Select(i => i.PlanId);
                var taskDataPlan = planData.Where(plan => plan.IsDeleted.Equals(false)
                    && (cmgnidlist.Count() > 0 ? cmgnidlist.Contains(plan.PlanId) : planList.Contains(plan.PlanId)))
                                                  .ToList()
                                                   .Select(plan => new
                                                   {
                                                       id = string.Format("L{0}", plan.PlanId),
                                                       text = plan.Title,
                                                       start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetStartDateForPlan(plan.PlanId, lstCampaign, plan.Year)),
                                                       duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                                                                 GetStartDateForPlan(plan.PlanId, lstCampaign, plan.Year),
                                                                                                 GetEndDateForPlan(plan.PlanId, lstCampaign, plan.Year)),
                                                       progress = GetProgressPerformance(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetStartDateForPlan(plan.PlanId, lstCampaign, plan.Year)),
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
                    machineName = "",
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
                    OwnerName = GetOwnerName(plan.CreatedBy),
                    Permission = IsPlanCreateAllAuthorized == false ? (plan.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(plan.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });

                #endregion

                var taskDataPlanMerged = newTaskDataPlan.Concat<object>(taskDataImprovementActivity).Concat<object>(taskDataImprovementTactic);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public double GetProgramProgress(List<Custom_Plan_Campaign_Program_Tactic> lstTactic, Custom_Plan_Campaign_Program planCampaignProgram, List<ProgressModel> lstImprovementTactic, int PlanId)
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
                var lstAffectedTactic = lstTactic.Where(tactic => tactic.IsDeleted.Equals(false) && tactic.PlanProgramId == planCampaignProgram.PlanProgramId && (tactic.StartDate >= minDate).Equals(true)
                                                        && tactic.PlanId == PlanId)
                                                .Select(tactic => tactic.StartDate)
                                                .ToList();
                if (lstAffectedTactic.Count > 0)
                {
                    //// minimum start Date of tactics
                    DateTime tacticMinStartDate = lstAffectedTactic.Min();
                    tacticMinStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tacticMinStartDate));
                    result = GetProgressResult(tacticMinStartDate, minDate, programStartDate, planCampaignProgram.StartDate, planCampaignProgram.EndDate);
                }
            }
            return result;
        }

        public double GetProgramProgressViewByPerformance(List<Custom_Plan_Campaign_Program_Tactic> lstTactic, Custom_Plan_Campaign_Program planCampaignProgram, DateTime effectiveMinDate)
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
                var lstAffectedTactic = lstTactic.Where(tactic => tactic.PlanProgramId == planCampaignProgram.PlanProgramId && (tactic.StartDate >= minDate).Equals(true)
                                                        )
                                               .Select(tactic => tactic.StartDate)
                                                .ToList();
                if (lstAffectedTactic.Count > 0)
                {
                    //// minimum start Date of tactics
                    DateTime tacticMinStartDate = lstAffectedTactic.Min();
                    tacticMinStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tacticMinStartDate));
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
        public double GetCampaignProgress(List<Custom_Plan_Campaign_Program_Tactic> lstTactic, Plan_Campaign planCampaign, List<ProgressModel> lstImprovementTactic, int PlanId)
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
                var lstAllTactic = lstTactic.Where(tactic => (tactic.EndDate < CalendarStartDate || tactic.StartDate > CalendarEndDate).Equals(false)
                                                    && tactic.PlanId == PlanId);

                //// List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = lstAllTactic.Where(tactic => (tactic.IsDeleted.Equals(false) && tactic.StartDate >= minDate).Equals(true) && tactic.PlanCampaignId == planCampaign.PlanCampaignId)
                                                    .Select(tactic => tactic.StartDate)
                                                     .ToList();

                if (lstAffectedTactic != null && lstAffectedTactic.Count > 0)
                {
                    //// minimum start Date of tactics
                    DateTime tacticMinStartDate = lstAffectedTactic.Min();
                    tacticMinStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tacticMinStartDate));
                    result = GetProgressResult(tacticMinStartDate, minDate, campaignStartDate, planCampaign.StartDate, planCampaign.EndDate);
                }
            }
            return result;
        }

        public double GetCampaignProgressViewByPerformance(List<Custom_Plan_Campaign_Program_Tactic> lstTactic, Plan_Campaign planCampaign, DateTime effectiveMinDate)
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
                var lstAllTactic = lstTactic.Where(tactic => (tactic.EndDate < CalendarStartDate || tactic.StartDate > CalendarEndDate).Equals(false));

                //// List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = lstAllTactic.Where(tactic => (tactic.StartDate >= minDate).Equals(true) && tactic.PlanCampaignId == planCampaign.PlanCampaignId)
                                                     .Select(tactic => tactic.StartDate)
                                                     .ToList();

                if (lstAffectedTactic != null && lstAffectedTactic.Count > 0)
                {
                    //// minimum start Date of tactics
                    DateTime tacticMinStartDate = lstAffectedTactic.Min();
                    tacticMinStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tacticMinStartDate));
                    result = (tacticMinStartDate >= minDate) ? GetProgressResult(tacticMinStartDate, minDate, campaignStartDate, planCampaign.StartDate, planCampaign.EndDate) : 0;
                }
            }
            return result;
        }

        public double GetProgressPerformance(string taskStartDate, double taskDuration, List<Custom_Plan_Campaign_Program_Tactic> lstTactic, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic, int PlanId)
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
                var lstAffectedTactic = lstTactic.Where(tactic => (tactic.StartDate >= minDate).Equals(true) && tactic.PlanId == PlanId)
                                               .Select(tactic => tactic.StartDate)
                                                 .ToList();
                if (lstAffectedTactic.Count > 0)
                {
                    //// Minimum start Date of tactics
                    DateTime tacticMinStartDate = lstAffectedTactic.Min();
                    tacticMinStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tacticMinStartDate));
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
        /// Function to get min start date using planID
        /// </summary>
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic and so on)</param>
        /// <param name="typeId">Selected Id as per selected GanttTab</param>
        /// <param name="planid">Planid of selected plan</param>
        /// <param name="lstCampaign">List of campaign </param>
        /// <param name="lstProgram">List of Program</param>
        /// <param name="lstTactic">List of Tactic</param>
        /// <returns>Return the min start date for program and Campaign</returns>
        public DateTime GetMinStartDateForPlan(GanttTabs currentGanttTab, int planId, List<Plan_Campaign> lstCampaign, List<Custom_Plan_Campaign_Program> lstProgram, List<Custom_Plan_Campaign_Program_Tactic> lstTactic)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime minDateTactic = DateTime.Now;

            //// Check the case with selected plan gantt type and if it's match then extract the min date from tactic list 
            switch (currentGanttTab)
            {
                case GanttTabs.Tactic:
                    queryPlanProgramId = lstTactic.Where(t => t.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    if (lstTactic.Count > 0 && lstTactic.Where(t => t.PlanId == planId).Count() > 0)
                    {
                        minDateTactic = lstTactic.Where(t => t.PlanId == planId).Select(t => t.StartDate).ToList().Min();
                    }
                    break;
                case GanttTabs.None:
                    queryPlanProgramId = lstProgram.Where(program => program.PlanId == planId).Select(program => program.PlanProgramId).ToList<int>();
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
        public DateTime GetMinStartDateForPlanOfCustomField(string currentGanttTab, string tacticstatus, string typeId, int planId, List<Plan_Campaign> lstCampaign, List<Custom_Plan_Campaign_Program> lstProgram, List<Custom_Plan_Campaign_Program_Tactic> lstTactic, bool IsCampaign = false, bool IsProgram = false)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime minDateTactic = DateTime.Now;
            PlanGanttTypes objPlanGanttTypes = (PlanGanttTypes)Enum.Parse(typeof(PlanGanttTypes), currentGanttTab, true);

            //// Check the case with selected plan gantt type and if it's match then extract the min date from tactic list
            switch (objPlanGanttTypes)
            {
                case PlanGanttTypes.Tactic:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.PlanProgramId).ToList<int>();
                    if (lstTactic.Count > 0)
                    {
                        minDateTactic = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.StartDate).Min();
                    }
                    break;
                case PlanGanttTypes.Stage:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.StageId == Convert.ToInt32(typeId) && tactic.PlanId == planId).Select(tactic => tactic.PlanProgramId).ToList<int>();
                    if (lstTactic.Count > 0)
                    {
                        minDateTactic = lstTactic.Where(tactic => tactic.StageId == Convert.ToInt32(typeId) && tactic.PlanId == planId).Select(tactic => tactic.StartDate).Min();
                    }
                    break;
                case PlanGanttTypes.Status:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.Status == tacticstatus && tactic.PlanId == planId).Select(tactic => tactic.PlanProgramId).ToList<int>();
                    if (lstTactic.Count > 0)
                    {
                        minDateTactic = lstTactic.Where(tactic => tactic.Status == tacticstatus && tactic.PlanId == planId).Select(tactic => tactic.StartDate).Min();
                    }
                    break;
                case PlanGanttTypes.Custom:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.PlanProgramId).ToList<int>();
                    if (lstTactic.Count > 0)
                    {
                        minDateTactic = lstTactic.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.PlanProgramId : tactic.PlanTacticId)) == Convert.ToInt32(typeId) && tactic.PlanId == planId).Select(tactic => tactic.StartDate).Min();
                    }
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
        /// Function to get max end date using planID
        /// </summary>
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic and so on)</param>
        /// <param name="typeId">Selected Id as per selected GanttTab</param>
        /// <param name="planid">Planid of selected plan</param>
        /// <param name="lstCampaign">List of campaign </param>
        /// <param name="lstProgram">List of Program</param>
        /// <param name="lstTactic">List of Tactic</param>
        /// <returns>Return the min start date for program and Campaign</returns>
        public DateTime GetMaxEndDateForPlan(GanttTabs currentGanttTab, int planId, List<Plan_Campaign> lstCampaign, List<Custom_Plan_Campaign_Program> lstProgram, List<Custom_Plan_Campaign_Program_Tactic> lstTactic)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime maxDateTactic = DateTime.Now;

            //// Check the case with selected plan gantt type and if it's match then extract the max date from tactic list
            switch (currentGanttTab)
            {
                case GanttTabs.Tactic:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.PlanProgramId).ToList<int>();
                    if (lstTactic.Count > 0 && lstTactic.Where(t => t.PlanId == planId).Count() > 0)
                    {
                        maxDateTactic = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.EndDate).Max();
                    }
                    break;
                case GanttTabs.None:
                    queryPlanProgramId = lstProgram.Where(program => program.PlanId == planId).Select(program => program.PlanProgramId).ToList<int>();
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
        public DateTime GetMaxEndDateForPlanOfCustomFields(string currentGanttTab, string tacticstatus, string typeId, int planId, List<Plan_Campaign> lstCampaign, List<Custom_Plan_Campaign_Program> lstProgram, List<Custom_Plan_Campaign_Program_Tactic> lstTactic, bool IsCampaign = false, bool IsProgram = false)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime maxDateTactic = DateTime.Now;
            PlanGanttTypes objPlanGanttTypes = (PlanGanttTypes)Enum.Parse(typeof(PlanGanttTypes), currentGanttTab, true);

            //// Check the case with selected plan gantt type and if it's match then extract the max date from tactic list
            switch (objPlanGanttTypes)
            {
                case PlanGanttTypes.Tactic:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.PlanProgramId).ToList<int>();
                    if (lstTactic.Count > 0)
                    {
                        maxDateTactic = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.EndDate).Max();
                    }
                    break;
                case PlanGanttTypes.Stage:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.StageId == Convert.ToInt32(typeId) && tactic.PlanId == planId).Select(tactic => tactic.PlanProgramId).ToList<int>();
                    if (lstTactic.Count > 0)
                    {
                        maxDateTactic = lstTactic.Where(tactic => tactic.StageId == Convert.ToInt32(typeId) && tactic.PlanId == planId).Select(tactic => tactic.EndDate).Max();
                    }
                    break;
                case PlanGanttTypes.Status:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.Status == tacticstatus && tactic.PlanId == planId).Select(tactic => tactic.PlanProgramId).ToList<int>();
                    if (lstTactic.Count > 0)
                    {
                        maxDateTactic = lstTactic.Where(tactic => tactic.Status == tacticstatus && tactic.PlanId == planId).Select(tactic => tactic.EndDate).Max();
                    }
                    break;
                case PlanGanttTypes.Custom:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.PlanProgramId).ToList<int>();
                    if (lstTactic.Count > 0)
                    {
                        maxDateTactic = lstTactic.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.PlanProgramId : tactic.PlanTacticId)) == Convert.ToInt32(typeId) && tactic.PlanId == planId).Select(tactic => tactic.EndDate).Max();
                    }
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

        public DateTime GetMaxEndDateStageAndBusinessUnitPerformance(List<Plan_Campaign> lstCampaign, List<Custom_Plan_Campaign_Program> lstProgram, List<Custom_Plan_Campaign_Program_Tactic> lstTactic)
        {
            //// Get list of program ids
            List<int> queryPlanProgramId = lstTactic.Select(tactic => tactic.PlanProgramId).ToList<int>();
            //// Get list of campaign ids
            List<int> queryPlanCampaignId = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.PlanCampaignId).ToList<int>();
            DateTime maxDateTactic, maxDateProgram, maxDateCampaign;
            maxDateTactic = maxDateProgram = maxDateCampaign = DateTime.Now;
            //// Get maximmum end date from tactic list
            if (lstTactic.Count > 0)
            {
                maxDateTactic = lstTactic.Select(tactic => tactic.EndDate).Max();
            }
            //// Get maximmum end date from program list
            if (lstProgram.Count > 0)
            {
                maxDateProgram = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.EndDate).Max();
            }
            //// Get maximmum end date from campaign list
            if (lstCampaign.Count > 0)
            {
                maxDateCampaign = lstCampaign.Where(campaign => queryPlanCampaignId.Contains(campaign.PlanCampaignId)).Select(campaign => campaign.EndDate).Max();
            }

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


        #region GetNumberOfActivityPerMonthByMultiplePlanId
        /// <summary>
        /// Get Number of Activity Per Month for Activity Distribution Chart for MultiplePlanIds for Home/Plan header.
        /// </summary>
        /// <param name="strPlanIds">Comma separated string of Plan Ids</param>
        /// <param name="strparam">Upcoming Activity dropdown selected option e.g. planyear, thisyear</param>
        /// <returns>returns Activity Chart object as jsonresult</returns>
        public async Task<JsonResult> GetNumberOfActivityPerMonth(string planid, string strparam, bool isMultiplePlan, string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "", string TabId = "", bool IsHeaderActuals = false)
        {
            List<int> filteredPlanIds = new List<int>();
            string planYear = string.Empty;
            int Planyear;
            bool isNumeric = false;
            List<int> campplanid = new List<int>();
            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;

            isNumeric = int.TryParse(strparam, out Planyear);
            if (isNumeric)
            {
                planYear = Convert.ToString(Planyear);
            }
            else
            {
                planYear = DateTime.Now.Year.ToString();
            }
            // Added by Arpita Soni on 05/26/2016 to resolve issue in copy tactic in inspect popup
            if (string.IsNullOrEmpty(strparam))
            {
                List<Plan> lstPlans = new List<Plan>();
                lstPlans = Common.GetPlan();
                int planId;
                Int32.TryParse(planid, out planId);
                Plan objPlan = lstPlans.Where(x => x.PlanId == planId).FirstOrDefault();
                if (objPlan != null)
                {
                    planYear = objPlan.Year;
                    strparam = planYear;
                }

            }
            //// Set start and end date for calender
            Common.GetPlanGanttStartEndDate(planYear, strparam, ref CalendarStartDate, ref CalendarEndDate);
            //Modified BY Komal rawal for #1929 proper Hud chart and count
            bool IsMultiYearPlan = false;
            if (isMultiplePlan)
            {


                List<int> planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(plan => int.Parse(plan)).ToList();
                // Add By Nishant Sheth
                // Desc :: for multiple extend plan #1750/#1761
                var planData = objDbMrpEntities.Plans.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false)).Select(a => a).ToList();
                // var CampaignList = planData.Select(ids => ids.Plan_Campaign).ToList();
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
                var Plan = objDbMrpEntities.Plans.FirstOrDefault(_plan => _plan.PlanId.Equals(PlanId));
                planYear = Plan.Year;
                var CampaignList = Plan.Plan_Campaign.Where(Camp => Camp.IsDeleted == false).ToList(); //Modified by komal to check is deleted flag


                if (CampaignList.Count > 0)
                {
                    // Added By komal Rawal for #1929 if plan is multiyear then activity distribution chart should nbe according to that in grid view
                    if (TabId == "liGrid")
                    {
                        int StartYear = CampaignList.Select(camp => camp.StartDate.Year).Min();
                        int EndYear = CampaignList.Select(camp => camp.EndDate.Year).Max();

                        if (EndYear != StartYear)
                        {
                            strparam = StartYear + "-" + EndYear;
                            IsMultiYearPlan = true;
                        }
                        else
                        {
                            strparam = Convert.ToString(StartYear);
                            Common.GetPlanGanttStartEndDate(planYear, strparam, ref CalendarStartDate, ref CalendarEndDate);
                        }


                    }
                }
                /// if strparam value null then set planYear as default value.
                if (string.IsNullOrEmpty(strparam))
                    strparam = planYear;
                isNumeric = int.TryParse(strparam, out Planyear);
                if (!string.IsNullOrEmpty(planid))
                    filteredPlanIds.Add(int.Parse(planid));

            }
            //Modified By Komal Rawal for #2059 display no tactics if filters are deselected
            //// Get planyear of the selected Plan
            if (strparam.Contains("-") || IsMultiYearPlan)
            {
                List<ActivityChart> lstActivityChartyears = new List<ActivityChart>();
                lstActivityChartyears = getmultipleyearActivityChart(strparam, planid, CustomFieldId, OwnerIds, TacticTypeids, StatusIds, isMultiplePlan, IsHeaderActuals);
                return Json(new { lstchart = lstActivityChartyears.ToList(), strparam = strparam }, JsonRequestBehavior.AllowGet);
            }
            //End

            var objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic =>
                                                   campplanid.Count > 0 ? campplanid.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) : filteredPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate)) && tactic.IsDeleted == false).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate, isdelete = tactic.IsDeleted }).ToList();

            objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(tactic => tactic.isdelete.Equals(false)).ToList();

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
            //Modified By Komal Rawal for #2059 display no tactics if filters are deselected
            if ((filterOwner.Count > 0 || filterTacticType.Count > 0 || filterStatus.Count > 0 || filteredCustomFields.Count > 0) && IsHeaderActuals != true)
            {
                lstTacticIds = objPlan_Campaign_Program_Tactic.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(pcptobj => (filterOwner.Contains(pcptobj.CreatedBy)) &&
                                         (filterTacticType.Contains(pcptobj.TacticTypeId)) &&
                                         (filterStatus.Contains(pcptobj.Status))).ToList();

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
                        //if (startDate.Year == System.DateTime.Now.Year)
                        // {
                        //differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));

                        //differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));
                        differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM-yyyy"));

                        List<string> thismonthdifferenceItem = new List<string>();
                        if (differenceItems.Count() > 12)
                        {
                            thismonthdifferenceItem = differenceItems.ToList();
                            thismonthdifferenceItem.RemoveRange(12, thismonthdifferenceItem.Count - 12);
                        }
                        else
                        {
                            thismonthdifferenceItem = differenceItems.ToList();
                        }




                        foreach (string objDifference in thismonthdifferenceItem)
                        {
                            string[] diffrenceitem = objDifference.Split('-');
                            monthNo = Convert.ToInt32(diffrenceitem[0].TrimStart('0'));
                            if (monthNo == DateTime.Now.Month)
                            {
                                if (diffrenceitem[1] == System.DateTime.Now.Year.ToString())
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
                                else
                                {
                                    monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                }
                            }
                        }
                        // }
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
            return Json(new { lstchart = lstActivityChart.ToList(), strparam = strparam }, JsonRequestBehavior.AllowGet); //Modified BY Komal rawal for #1929 proper Hud chart and count
        }

        public async Task<JsonResult> GetNumberOfActivityPerMonthPer(string planid, string strparam, bool isMultiplePlan, string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "", string TabId = "", bool IsHeaderActuals = false)
        {

            List<int> filteredPlanIds = new List<int>();
            string planYear = string.Empty;
            int Planyear;
            bool isNumeric = false;
            List<int> campplanid = new List<int>();
            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;

            isNumeric = int.TryParse(strparam, out Planyear);
            if (isNumeric)
            {
                planYear = Convert.ToString(Planyear);
            }
            else
            {
                planYear = DateTime.Now.Year.ToString();
            }
            // Added by Arpita Soni on 05/26/2016 to resolve issue in copy tactic in inspect popup
            if (string.IsNullOrEmpty(strparam))
            {
                List<Plan> lstPlans = new List<Plan>();
                lstPlans = Common.GetPlan();
                int planId;
                Int32.TryParse(planid, out planId);
                Plan objPlan = lstPlans.Where(x => x.PlanId == planId).FirstOrDefault();
                if (objPlan != null)
                {
                    planYear = objPlan.Year;
                    strparam = planYear;
                }

            }

            //// Set start and end date for calender
            Common.GetPlanGanttStartEndDate(planYear, strparam, ref CalendarStartDate, ref CalendarEndDate);
            DataSet dsPlanCampProgTac = new DataSet();
            dsPlanCampProgTac = (DataSet)objCache.Returncache(Enums.CacheObject.dsPlanCampProgTac.ToString());
            var planData = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
            var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]);
            //Modified BY Komal rawal for #1929 proper Hud chart and count
            bool IsMultiYearPlan = false;
            if (isMultiplePlan)
            {

                List<int> planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(plan => int.Parse(plan)).ToList();
                // Add By Nishant Sheth
                // Desc :: for multiple extend plan #1750/#1761
                //var planData = objDbMrpEntities.Plans.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false)).Select(a => a).ToList();

                // var CampaignList = planData.Select(ids => ids.Plan_Campaign).ToList();
                var planList = planData.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && plan.Year == planYear).Select(a => a.PlanId).ToList();

                if (planList.Count == 0)
                {
                    campplanid = lstCampaign.Where(camp => !(camp.StartDate > CalendarEndDate || camp.EndDate < CalendarStartDate) && planIds.Contains(camp.PlanId)).Select(a => a.PlanId).Distinct().ToList();
                }
                filteredPlanIds = planData.Where(plan => plan.IsDeleted == false &&
                    campplanid.Count > 0 ? campplanid.Contains(plan.PlanId) : planIds.Contains(plan.PlanId)).ToList().Select(plan => plan.PlanId).ToList();

            }
            else
            {
                int PlanId = !string.IsNullOrEmpty(planid) ? int.Parse(planid) : 0;
                //// Get planyear of the selected Plan
                var Plan = planData.FirstOrDefault(_plan => _plan.PlanId.Equals(PlanId));
                //Added By Maitri Gandhi for #2167
                if (Plan == null)
                {
                    Plan = objDbMrpEntities.Plans.FirstOrDefault(_plan => _plan.PlanId.Equals(PlanId));
                }
                planYear = Plan.Year;
                var CampaignList = lstCampaign.ToList(); //Modified by komal to check is deleted flag


                if (CampaignList.Count > 0)
                {
                    // Added By komal Rawal for #1929 if plan is multiyear then activity distribution chart should nbe according to that in grid view
                    if (TabId == "liGrid")
                    {
                        int StartYear = CampaignList.Select(camp => camp.StartDate.Year).Min();
                        int EndYear = CampaignList.Select(camp => camp.EndDate.Year).Max();

                        if (EndYear != StartYear)
                        {
                            strparam = StartYear + "-" + EndYear;
                            IsMultiYearPlan = true;
                        }
                        else
                        {
                            strparam = Convert.ToString(StartYear);
                            Common.GetPlanGanttStartEndDate(planYear, strparam, ref CalendarStartDate, ref CalendarEndDate);
                        }

                    }
                }
                /// if strparam value null then set planYear as default value.
                if (string.IsNullOrEmpty(strparam))
                    strparam = planYear;
                isNumeric = int.TryParse(strparam, out Planyear);
                if (!string.IsNullOrEmpty(planid))
                    filteredPlanIds.Add(int.Parse(planid));

            }
            //Modified By Komal Rawal for #2059 display no tactics if filters are deselected
            //// Get planyear of the selected Plan
            if (strparam.Contains("-") || IsMultiYearPlan)
            {

                List<ActivityChart> lstActivityChartyears = new List<ActivityChart>();
                lstActivityChartyears = getmultipleyearActivityChartPer(strparam, planid, CustomFieldId, OwnerIds, TacticTypeids, StatusIds, isMultiplePlan, IsHeaderActuals);

                return Json(new { lstchart = lstActivityChartyears.ToList(), strparam = strparam }, JsonRequestBehavior.AllowGet);
            }
            //End

            //var objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic =>
            //                                       campplanid.Count > 0 ? campplanid.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) : filteredPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate)) && tactic.IsDeleted == false).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate, isdelete = tactic.IsDeleted }).ToList();
            //var objPlan_Campaign_Program_Tactic = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]).Where(tactic =>
            //                                       campplanid.Count > 0 ? campplanid.Contains(tactic.PlanId) : filteredPlanIds.Contains(tactic.PlanId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate)) && tactic.IsDeleted == false).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate, isdelete = tactic.IsDeleted }).ToList();
            //Modified by Rahul Shah on 14/04/2016 for PL #2110
            var objPlan_Campaign_Program_Tactic = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]).Where(tactic =>
                                                   campplanid.Count > 0 ? campplanid.Contains(tactic.PlanId) : filteredPlanIds.Contains(tactic.PlanId) && tactic.EndDate > CalendarStartDate && tactic.StartDate < CalendarEndDate && tactic.IsDeleted == false).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate, isdelete = tactic.IsDeleted }).ToList();

            objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(tactic => tactic.isdelete.Equals(false)).ToList();

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
            //Modified By Komal Rawal for #2059 display no tactics if filters are deselected
            if ((filterOwner.Count > 0 || filterTacticType.Count > 0 || filterStatus.Count > 0 || filteredCustomFields.Count > 0) && IsHeaderActuals != true)
            {
                lstTacticIds = objPlan_Campaign_Program_Tactic.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(pcptobj => (filterOwner.Contains(pcptobj.CreatedBy)) &&
                                         (filterTacticType.Contains(pcptobj.TacticTypeId)) &&
                                         (filterStatus.Contains(pcptobj.Status))).ToList();

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
                //foreach (var tactic in objPlan_Campaign_Program_Tactic)
                int TacticCount = objPlan_Campaign_Program_Tactic.Count;
                for (int tactic = 0; tactic < TacticCount; tactic++)
                {
                    startDate = endDate = new DateTime();
                    startDate = Convert.ToDateTime(objPlan_Campaign_Program_Tactic[tactic].StartDate);
                    endDate = Convert.ToDateTime(objPlan_Campaign_Program_Tactic[tactic].EndDate);

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
                        //if (startDate.Year == System.DateTime.Now.Year)
                        // {
                        //differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));

                        //differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));
                        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                        differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM-yyyy"));

                        List<string> thismonthdifferenceItem = new List<string>();
                        //if (differenceItems.Count() > 12)
                        //{
                        //    thismonthdifferenceItem = differenceItems.ToList();
                        //    thismonthdifferenceItem.RemoveRange(12, thismonthdifferenceItem.Count - 12);
                        //}
                        //else
                        //{
                        //    thismonthdifferenceItem = differenceItems.ToList();
                        //}

                        //foreach (string objDifference in thismonthdifferenceItem)
                        thismonthdifferenceItem = differenceItems.ToList();
                        int thismonthdifferenceItemCount = thismonthdifferenceItem.Count;
                        for (int monthdiffer = 0; monthdiffer < thismonthdifferenceItemCount; monthdiffer++)
                        {
                            string[] diffrenceitem = thismonthdifferenceItem[monthdiffer].Split('-');
                            monthNo = Convert.ToInt32(diffrenceitem[0].TrimStart('0'));
                            if (monthNo == DateTime.Now.Month)
                            {
                                if (diffrenceitem[1] == System.DateTime.Now.Year.ToString())
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
                                //else
                                //{
                                //    monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                //}
                            }
                        }
                        // }
                    }
                    else if (strparam.Equals(Enums.UpcomingActivities.thisquarter.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (startDate.Year == System.DateTime.Now.Year || endDate.Year == System.DateTime.Now.Year)
                        {
                            if (currentMonth == 1 || currentMonth == 2 || currentMonth == 3)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q1), startDate, endDate, monthArray);
                                //if (startDate.Month == 1 || startDate.Month == 2 || startDate.Month == 3 || endDate.Month == 1 || endDate.Month == 2 || endDate.Month == 3)
                                //{
                                //    monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q1), startDate, endDate, monthArray);
                                //}
                            }
                            else if (currentMonth == 4 || currentMonth == 5 || currentMonth == 6)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q2), startDate, endDate, monthArray);
                                //if (startDate.Month == 4 || startDate.Month == 5 || startDate.Month == 6 || endDate.Month == 4 || endDate.Month == 5 || endDate.Month == 6)
                                //{
                                //    monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q2), startDate, endDate, monthArray);
                                //}
                            }
                            else if (currentMonth == 7 || currentMonth == 8 || currentMonth == 9)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q3), startDate, endDate, monthArray);
                                //if (startDate.Month == 7 || startDate.Month == 8 || startDate.Month == 9 || endDate.Month == 7 || endDate.Month == 8 || endDate.Month == 9)
                                //{
                                //    monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q3), startDate, endDate, monthArray);
                                //}
                            }
                            else if (currentMonth == 10 || currentMonth == 11 || currentMonth == 12)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q4), startDate, endDate, monthArray);
                                //if (startDate.Month == 10 || startDate.Month == 11 || startDate.Month == 12 || endDate.Month == 10 || endDate.Month == 11 || endDate.Month == 12)
                                //{
                                //    monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q4), startDate, endDate, monthArray);
                                //}
                            }
                        }
                    }
                }
            }

            //// Prepare Activity Chart list
            List<ActivityChart> lstActivityChart = new List<ActivityChart>();
            int MonthCount = monthArray.Count();
            for (int month = 0; month < MonthCount; month++)
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
            return Json(new { lstchart = lstActivityChart.ToList(), strparam = strparam }, JsonRequestBehavior.AllowGet); //Modified BY Komal rawal for #1929 proper Hud chart and count
        }

        private List<ActivityChart> getmultipleyearActivityChart(string strParam, string planid, string CustomFieldId, string OwnerIds, string TacticTypeids, string StatusIds, bool isMultiplePlan, bool IsHeaderActuals)
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

                    //var objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
                    //                              campplanid.Count > 0 ? campplanid.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) : filteredPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate))).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate }).ToList();

                    var objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic =>
                                                 campplanid.Count > 0 ? campplanid.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) : filteredPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate)) && tactic.IsDeleted == false).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate, isdelete = tactic.IsDeleted }).ToList();

                    objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(tactic => tactic.isdelete.Equals(false)).ToList();

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
                    //Modified By Komal Rawal for #2059 display no tactics if filters are deselected
                    if ((filterOwner.Count > 0 || filterTacticType.Count > 0 || filterStatus.Count > 0 || filteredCustomFields.Count > 0) && IsHeaderActuals != true)
                    {
                        lstTacticIds = objPlan_Campaign_Program_Tactic.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                        objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(pcptobj => (filterOwner.Contains(pcptobj.CreatedBy)) &&
                                                 (filterTacticType.Contains(pcptobj.TacticTypeId)) &&
                                                 (filterStatus.Contains(pcptobj.Status))).ToList();

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
                    // int[] monthArray = new int[12];
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


                                        //if (monthNo == 1)
                                        //{
                                        //    monthArray[0] = monthArray[0] + 1;
                                        //}
                                        //else
                                        //{
                                        //    monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                        //}
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

        private List<ActivityChart> getmultipleyearActivityChartPer(string strParam, string planid, string CustomFieldId, string OwnerIds, string TacticTypeids, string StatusIds, bool isMultiplePlan, bool IsHeaderActuals = false)
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
            DataSet dsPlanCampProgTac = new DataSet();
            dsPlanCampProgTac = (DataSet)objCache.Returncache(Enums.CacheObject.dsPlanCampProgTac.ToString());
            if (strParam.Contains("-"))
            {
                if (isMultiplePlan)
                {
                    planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(plan => int.Parse(plan)).ToList();
                    //planData = objDbMrpEntities.Plans.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false)).Select(a => a).ToList();
                }
                planData = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]);
                string[] multipleyear = strParam.Split('-');
                int multiYearCount = multipleyear.Count();
                for (int i = 0; i < multiYearCount; i++)
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
                            campplanid = lstCampaign.Where(camp => !(camp.StartDate > CalendarEndDate || camp.EndDate < CalendarStartDate) && planIds.Contains(camp.PlanId)).Select(a => a.PlanId).Distinct().ToList();
                        }
                        filteredPlanIds = planData.Where(plan => plan.IsDeleted == false &&
                            campplanid.Count > 0 ? campplanid.Contains(plan.PlanId) : planIds.Contains(plan.PlanId)).ToList().Select(plan => plan.PlanId).ToList();
                    }
                    else
                    {

                        int PlanId = !string.IsNullOrEmpty(planid) ? int.Parse(planid) : 0;
                        //// Get planyear of the selected Plan
                        //planYear = planData.FirstOrDefault(_plan => _plan.PlanId.Equals(PlanId)).Year;
                        //Modified By Maitri Gandhi for #2167
                        var Plan = planData.FirstOrDefault(_plan => _plan.PlanId.Equals(PlanId));
                        if (Plan == null)
                        {
                            Plan = objDbMrpEntities.Plans.FirstOrDefault(_plan => _plan.PlanId.Equals(PlanId));
                        }
                        planYear = Plan.Year;
                        /// if strparam value null then set planYear as default value.
                        if (string.IsNullOrEmpty(multipleyear[i]))
                            multipleyear[i] = planYear;
                        isNumeric = int.TryParse(multipleyear[i], out Planyear);
                        if (!string.IsNullOrEmpty(planid))
                            filteredPlanIds.Add(int.Parse(planid));
                    }

                    //// Selecte tactic(s) from selected programs

                    //var objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
                    //                              campplanid.Count > 0 ? campplanid.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) : filteredPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate))).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate }).ToList();

                    var objPlan_Campaign_Program_Tactic = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]).Where(tactic =>
                                                 campplanid.Count > 0 ? campplanid.Contains(tactic.PlanId) : filteredPlanIds.Contains(tactic.PlanId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate)) && tactic.IsDeleted == false).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate, isdelete = tactic.IsDeleted }).ToList();

                    objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(tactic => tactic.isdelete.Equals(false)).ToList();

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
                    //Modified By Komal Rawal for #2059 display no tactics if filters are deselected
                    if ((filterOwner.Count > 0 || filterTacticType.Count > 0 || filterStatus.Count > 0 || filteredCustomFields.Count > 0) && IsHeaderActuals != true)
                    {
                        lstTacticIds = objPlan_Campaign_Program_Tactic.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                        objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(pcptobj => (filterOwner.Contains(pcptobj.CreatedBy)) &&
                                                 (filterTacticType.Contains(pcptobj.TacticTypeId)) &&
                                                 (filterStatus.Contains(pcptobj.Status))).ToList();

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
                    // int[] monthArray = new int[12];
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
                        //foreach (var tactic in objPlan_Campaign_Program_Tactic)
                        int TacticCount = objPlan_Campaign_Program_Tactic.Count;
                        for (int tactic = 0; tactic < TacticCount; tactic++)
                        {
                            startDate = endDate = new DateTime();
                            startDate = Convert.ToDateTime(objPlan_Campaign_Program_Tactic[tactic].StartDate);
                            endDate = Convert.ToDateTime(objPlan_Campaign_Program_Tactic[tactic].EndDate);

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
                                    int differenceItemsCount = differenceItems.Count();
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


                                        //if (monthNo == 1)
                                        //{
                                        //    monthArray[0] = monthArray[0] + 1;
                                        //}
                                        //else
                                        //{
                                        //    monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                        //}
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

                                        }
                                    }
                                }
                            }
                        }
                    }

                    //// Prepare Activity Chart list
                    string quater = string.Empty;
                    lstActivityChart = new List<ActivityChart>();
                    int MonthCount = monthArrayactivity.Count();
                    for (int month = 0; month < MonthCount; month++)
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
            //if (startDateParam.Month == month1 || startDateParam.Month == month2 || startDateParam.Month == month3 || endDateParam.Month == month1 || endDateParam.Month == month2 || endDateParam.Month == month3)
            //{
            endDateParam = new DateTime(endDateParam.Year, endDateParam.Month, DateTime.DaysInMonth(endDateParam.Year, endDateParam.Month));
            differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDateParam.AddMonths(element)).TakeWhile(element => element <= endDateParam).Select(element => element.ToString("MM-yyyy"));


            List<string> thismonthdifferenceItem = new List<string>();
            if (differenceItems.Count() > 12)
            {
                thismonthdifferenceItem = differenceItems.ToList();
                thismonthdifferenceItem.RemoveRange(12, thismonthdifferenceItem.Count - 12);
            }
            else
            {
                thismonthdifferenceItem = differenceItems.ToList();
            }

            //
            //foreach (string objDifference in thismonthdifferenceItem)
            //{
            //    string[] diffrenceitem = objDifference.Split('-');
            //    monthNo = Convert.ToInt32(diffrenceitem[0].TrimStart('0'));
            //    if (monthNo == DateTime.Now.Month)
            //    {
            //        if (diffrenceitem[1] == System.DateTime.Now.Year.ToString())
            //        {
            //            if (monthNo == 1)
            //            {
            //                monthArray[0] = monthArray[0] + 1;
            //            }
            //        }
            //        else
            //        {
            //            monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
            //        }
            //    }
            //}
            //

            foreach (string d in thismonthdifferenceItem)
            {
                string[] diffrenceitem = d.Split('-');
                monthNo = Convert.ToInt32(diffrenceitem[0].TrimStart('0'));
                if (monthNo == month1 || monthNo == month2 || monthNo == month3)
                {
                    if (diffrenceitem[1] == System.DateTime.Now.Year.ToString())
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
                    //else
                    //{
                    //    monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                    //}
                }
            }

            endDateParam = endDate;
            //}           

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
            string stageMQL = Enums.Stage.MQL.ToString();

            // Add By Nishant Sheth
            // Desc :: To get performance regarding #2111 add stagelist into cache memory
            CacheObject dataCache = new CacheObject();
            List<Stage> stageList = dataCache.Returncache(Enums.CacheObject.StageList.ToString()) as List<Stage>;
            if (stageList == null)
            {
                stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
            }
            dataCache.AddCache(Enums.CacheObject.StageList.ToString(), stageList);
            int levelMQL = stageList.FirstOrDefault(s => s.ClientId.Equals(Sessions.User.ClientId) && s.IsDeleted == false && s.Code.Equals(stageMQL)).Level.Value;

            List<Plan_Campaign_Program_Tactic> TacticList = new List<Plan_Campaign_Program_Tactic>();
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            if (status == 0)
            {
                //// Modified By: Maninder Singh for TFS Bug#282: Extra Tactics Displaying in Add Actual Screen
                TacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(planTactic => planTactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(PlanId) &&
                                                                                tacticStatus.Contains(planTactic.Status) && planTactic.IsDeleted.Equals(false) &&
                                                                                !planTactic.Plan_Campaign_Program_Tactic_Actual.Any() &&
                                                                                (filteredTacticTypeIds.Contains(planTactic.TacticType.TacticTypeId)) &&
                                                                                (filteredOwner.Contains(planTactic.CreatedBy))).ToList();
            }
            else
            {
                TacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == PlanId &&
                                                                                tacticStatus.Contains(tactic.Status) && tactic.IsDeleted == false &&
                                                                                (filteredTacticTypeIds.Contains(tactic.TacticType.TacticTypeId)) &&
                                                                                (filteredOwner.Contains(tactic.CreatedBy))).ToList();
            }

            List<int> TacticIds = TacticList.Select(tactic => tactic.PlanTacticId).ToList();
            List<string> lstFilteredCustomFieldOptionIds = new List<string>();
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
                        lstFilteredCustomFieldOptionIds.Add(splittedCustomField[1]);
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
                    //Commented and Added by Rahul Shah on 02/05/2016 for PL #2111
                    //modifiedBy = Common.TacticModificationMessage(tactic.PlanTacticId, userName),
                    modifiedBy = Common.TacticModificationMessageActual(tactic.PlanTacticId, userName, tacticLineItemActual, tacticActuals),

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
                    //Commented and Added by Rahul Shah on 02/05/2016 for PL #2111
                    //stageTitle = Common.GetTacticStageTitle(tactic.PlanTacticId),
                    stageTitle = Common.GetTacticStageTitleActual(tactic.PlanTacticId, Convert.ToInt32(tactic.Stage.Level), levelMQL),
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
                }).ToList();

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
                }).ToList();

                var openTactics = lstTactic.Where(tactic => tactic.actualData.ToList().Count == 0).OrderBy(tactic => tactic.title).ToList();
                var allTactics = lstTactic.Where(tactic => tactic.actualData.ToList().Count != 0).ToList();

                var result = openTactics.Concat(allTactics).ToList();

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
        public List<OwnerModel> GetOwnerList(string ViewBy, string ActiveMenu, List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstAllowedEntityIds)               //modified by rahul shah for PL #2032. display only tactic owner in left pane
        {
            var lstOwners = GetIndividualsByPlanId(ViewBy, ActiveMenu, tacticList, lstAllowedEntityIds);
            //var lstOwners = GetIndividualsByPlanId(ViewBy, ActiveMenu, tacticList, lstAllowedEntityIds, otherownerids);
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
        /// modified by rahul shah for PL #2032. display only tactic owner in left pane
        public async Task<JsonResult> GetOwnerListForFilter(string PlanId, string ViewBy, string ActiveMenu)
        {
            try
            {
                List<int> lstAllowedEntityIds = new List<int>();
                List<int> PlanIds = string.IsNullOrWhiteSpace(PlanId) ? new List<int>() : PlanId.Split(',').Select(plan => int.Parse(plan)).ToList();
                // Add By Nishant Sheth
                // Desc :: Get records from Stored procedure for plan,campaign,program and tactic
                DataSet dsPlanCampProgTac = new DataSet();
                dsPlanCampProgTac = (DataSet)objCache.Returncache(Enums.CacheObject.dsPlanCampProgTac.ToString());
                //List<Guid> planownerids = objDbMrpEntities.Plans.Where(plan => PlanIds.Contains(plan.PlanId)).Select(plan => plan.CreatedBy).Distinct().ToList<Guid>();
                //// Select Tactics of selected plans
                //var campaignList = objDbMrpEntities.Plan_Campaign.Where(campaign => campaign.IsDeleted.Equals(false) && PlanIds.Contains(campaign.PlanId)).ToList();
                // Add By Nishant Sheth
                // Desc :: Get data from stored procedure results
                //List<Plan_Campaign> campaignList = dsPlanCampProgTac.Tables[1].AsEnumerable().Select(row => new Plan_Campaign
                //{
                //    Abbreviation = Convert.ToString(row["Abbreviation"]),
                //    CampaignBudget = Convert.ToDouble(row["CampaignBudget"]),
                //    CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
                //    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                //    Description = Convert.ToString(row["Description"]),
                //    EndDate = Convert.ToDateTime(row["EndDate"]),
                //    IntegrationInstanceCampaignId = Convert.ToString(row["IntegrationInstanceCampaignId"]),
                //    IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                //    IsDeployedToIntegration = Convert.ToBoolean(row["IsDeployedToIntegration"]),
                //    LastSyncDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["LastSyncDate"])) ? (DateTime?)null : row["LastSyncDate"]),
                //    ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                //    ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                //    PlanCampaignId = Convert.ToInt32(row["PlanCampaignId"]),
                //    PlanId = Convert.ToInt32(row["PlanId"]),
                //    StartDate = Convert.ToDateTime(row["StartDate"]),
                //    Status = Convert.ToString(row["Status"]),
                //    Title = Convert.ToString(row["Title"])
                //}).ToList();
                List<Plan_Campaign> campaignList = new List<Plan_Campaign>();
                if (dsPlanCampProgTac != null && dsPlanCampProgTac.Tables[1] != null)
                {
                    campaignList = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]);
                }

                objCache.AddCache(Enums.CacheObject.Campaign.ToString(), campaignList);

                var campaignListids = campaignList.Select(campaign => campaign.PlanCampaignId).ToList();
                //List<Guid> campaignownerids = campaignList.Select(campaign => campaign.CreatedBy).Distinct().ToList();
                //var programList = objDbMrpEntities.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false) && campaignListids.Contains(program.PlanCampaignId)).ToList();
                // Add By Nishant Sheth
                // Desc :: get preogram records from stored procedure results
                //List<Plan_Campaign_Program> programList = dsPlanCampProgTac.Tables[2].AsEnumerable().Select(row => new Plan_Campaign_Program
                //{
                //    Abbreviation = Convert.ToString(row["Abbreviation"]),
                //    CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
                //    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                //    Description = Convert.ToString(row["Description"]),
                //    EndDate = Convert.ToDateTime(row["EndDate"]),
                //    IntegrationInstanceProgramId = Convert.ToString(row["IntegrationInstanceProgramId"]),
                //    IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                //    IsDeployedToIntegration = Convert.ToBoolean(row["IsDeployedToIntegration"]),
                //    LastSyncDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["LastSyncDate"])) ? (DateTime?)null : row["LastSyncDate"]),
                //    ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                //    ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                //    PlanCampaignId = Convert.ToInt32(row["PlanCampaignId"]),
                //    PlanProgramId = Convert.ToInt32(row["PlanProgramId"]),
                //    ProgramBudget = Convert.ToDouble(row["ProgramBudget"]),
                //    StartDate = Convert.ToDateTime(row["StartDate"]),
                //    Status = Convert.ToString(row["Status"]),
                //    Title = Convert.ToString(row["Title"])
                //}).ToList();
                List<Plan_Campaign_Program> programList = new List<Plan_Campaign_Program>();
                if (dsPlanCampProgTac != null && dsPlanCampProgTac.Tables[2] != null)
                {
                    programList = Common.GetSpProgramList(dsPlanCampProgTac.Tables[2]);
                }

                objCache.AddCache(Enums.CacheObject.Program.ToString(), programList);

                var programListids = programList.Select(program => program.PlanProgramId).ToList();
                //List<Guid> programownerids = programList.Select(program => program.CreatedBy).Distinct().ToList();
                // Add By Nishant Sheth
                // Get Records from cache memory
                List<Custom_Plan_Campaign_Program_Tactic> customtacticList = (List<Custom_Plan_Campaign_Program_Tactic>)objCache.Returncache(Enums.CacheObject.CustomTactic.ToString());
                List<Plan_Campaign_Program_Tactic> tacticList = (List<Plan_Campaign_Program_Tactic>)objCache.Returncache(Enums.CacheObject.Tactic.ToString());
                //var tacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && programListids.Contains(tactic.PlanProgramId)).Select(tactic => tactic).ToList();
                //List<int> planTacticIds = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
                //List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                //Added by Rahul Shah on 06/01/2016 for PL#1854.
                string section = Enums.Section.Tactic.ToString();


                var customfield = objDbMrpEntities.CustomFields.Where(customField => customField.EntityType == section && customField.ClientId == Sessions.User.ClientId && customField.IsDeleted == false).ToList();
                objCache.AddCache(Enums.CacheObject.CustomField.ToString(), customfield);
                var customfieldidlist = customfield.Select(c => c.CustomFieldId).ToList();
                var lstAllTacticCustomFieldEntitiesanony = objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => customfieldidlist.Contains(customFieldEntity.CustomFieldId))
                                                                                       .Select(customFieldEntity => new CacheCustomField { EntityId = customFieldEntity.EntityId, CustomFieldId = customFieldEntity.CustomFieldId, Value = customFieldEntity.Value, CreatedBy = customFieldEntity.CreatedBy, CustomFieldEntityId = customFieldEntity.CustomFieldEntityId }).Distinct().ToList();

                //List<CustomField_Entity> lstAllTacticCustomFieldEntitiesanony = objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => customfieldidlist.Contains(customFieldEntity.CustomFieldId))
                //                                                                       .Select(customFieldEntity => new CustomField_Entity { EntityId = customFieldEntity.EntityId, CustomFieldId = customFieldEntity.CustomFieldId, Value = customFieldEntity.Value }).Distinct().ToList();
                //var lstAllTacticCustomFieldEntitiesanony = objSp.GetCustomFieldEntityList(string.Join(",", customfieldidlist));
                objCache.AddCache(Enums.CacheObject.CustomFieldEntity.ToString(), lstAllTacticCustomFieldEntitiesanony);
                // Get owner of all entity
                //Commented by Rahul Shah for PL #2032 on 16/03/2016
                //List<Guid> otherownerids = planownerids.Concat(campaignownerids).Concat(programownerids).Distinct().ToList();

                //foreach (var pId in PlanIds)
                //{
                //    List<int> planTacticIds = customtacticList.Where(tact => tact.PlanId == pId).Select(tact => tact.PlanTacticId).ToList();
                //    List<CustomField_Entity> customfieldlist = (from tbl in lstAllTacticCustomFieldEntitiesanony
                //                                                join lst in planTacticIds on tbl.EntityId equals lst
                //                                                select new CustomField_Entity
                //                                                {
                //                                                    EntityId = tbl.EntityId,
                //                                                    CustomFieldId = tbl.CustomFieldId,
                //                                                    Value = tbl.Value
                //                                                }).ToList();

                //    List<int> AllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false, customfieldlist);

                //    if (AllowedEntityIds.Count > 0)
                //    {
                //        lstAllowedEntityIds.AddRange(AllowedEntityIds);
                //    }
                //}
                for (int i = 0; i < PlanIds.Count; i++)
                {
                    List<int> planTacticIds = customtacticList.Where(tact => tact.PlanId == PlanIds[i]).Select(tact => tact.PlanTacticId).ToList();
                    List<CustomField_Entity> customfieldlist = (from tbl in lstAllTacticCustomFieldEntitiesanony
                                                                join lst in planTacticIds on tbl.EntityId equals lst
                                                                select new CustomField_Entity
                                                                {
                                                                    EntityId = tbl.EntityId,
                                                                    CustomFieldId = tbl.CustomFieldId,
                                                                    Value = tbl.Value
                                                                }).ToList();

                    List<int> AllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false, customfieldlist);
                    if (AllowedEntityIds.Count > 0)
                    {
                        lstAllowedEntityIds.AddRange(AllowedEntityIds);
                    }

                }

                //Added by Nishant to bring the owner name in the list even if they dont own any tactic
                var LoggedInUser = new OwnerModel
                {
                    OwnerId = Sessions.User.UserId.ToString(),
                    Title = Convert.ToString(Sessions.User.FirstName + " " + Sessions.User.LastName),
                };
                await Task.Delay(1);
                //return Json(new { isSuccess = true, AllowedOwner = GetOwnerList(ViewBy, ActiveMenu, tacticList, lstAllowedEntityIds, otherownerids), LoggedInUser = LoggedInUser }, JsonRequestBehavior.AllowGet);
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
        //modified by rahul shah for PL #2032. display only tactic owner in left pane
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
                //TacticUserList = TacticUserList.Where(tactic => !ViewBy.Equals(GanttTabs.Request.ToString())).Distinct().ToList();

                if (TacticUserList.Count > 0)
                {
                    //// Custom Restrictions applied
                    TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.UserId).ToList();
                }
                var useridslist = TacticUserList.Select(tactic => tactic.CreatedBy).Distinct().ToList();
                //var useridslist = otherownerids.Concat(TacticUserList.Select(tactic => tactic.CreatedBy)).Distinct().ToList();
                string strContatedIndividualList = string.Join(",", useridslist.Select(tactic => tactic.ToString()));
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
                // Modified By Nishant
                if (ViewBy.Equals(GanttTabs.Request.ToString()))
                {

                    TacticUserList = TacticUserList.Where(tactic => status.Contains(tactic.Status)).Distinct().ToList();

                }
                // End By Nishant Sheth


                if (TacticUserList.Count > 0)
                {
                    // Custom Restrictions applied
                    TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.UserId).ToList();
                }
                var useridslist = TacticUserList.Select(tactic => tactic.CreatedBy).Distinct().ToList();
                //var useridslist = otherownerids.Concat(TacticUserList.Select(tactic => tactic.CreatedBy)).Distinct().ToList();
                string strContatedIndividualList = string.Join(",", useridslist.Select(tactic => tactic.ToString()));
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
        public JsonResult BindUpcomingActivitesValues(string planids, string fltrYears)
        {
            //// Fetch the list of Upcoming Activity
            List<SelectListItem> objUpcomingActivity = UpComingActivity(planids, fltrYears);

            //bool IsItemExists = objUpcomingActivity.Where(activity => activity.Value == CurrentTime).Any();

            //if (IsItemExists)
            //{
            //    foreach (SelectListItem activity in objUpcomingActivity)
            //    {
            //        activity.Selected = false;
            //        //// Set it Selected ture if we found current time value in the list.
            //        if (CurrentTime == activity.Value)
            //        {
            //            activity.Selected = true;
            //        }
            //    }
            //}

            objUpcomingActivity = objUpcomingActivity.Where(activity => !string.IsNullOrEmpty(activity.Text)).OrderBy(activity => activity.Text, new AlphaNumericComparer()).ToList();
            return Json(objUpcomingActivity.ToList(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Function to process and fetch the Upcoming Activity  
        /// </summary>
        /// <param name="PlanIds">comma sepreated string plan id(s)</param>
        /// <returns>List fo SelectListItem of Upcoming activity</returns>
        public List<SelectListItem> UpComingActivity(string PlanIds, string fltrYears)
        {
            //// List of plan id(s)
            List<int> planIds = string.IsNullOrWhiteSpace(PlanIds) ? new List<int>() : PlanIds.Split(',').Select(plan => int.Parse(plan)).ToList();

            //// Fetch the active plan based of plan ids
            List<Plan> activePlan = objDbMrpEntities.Plans.Where(plan => planIds.Contains(plan.PlanId) && plan.IsActive.Equals(true) && plan.IsDeleted == false).ToList();
            List<int> lstplanid = activePlan.Select(a => a.PlanId).ToList();
            var lstCampaign = objDbMrpEntities.Plan_Campaign.Where(a => lstplanid.Contains(a.PlanId) && a.IsDeleted == false).ToList();
            var PlanYearList = activePlan.Select(plan => plan.Year).Distinct().ToList();

            //// Get the Current year and Pre define Upcoming Activites.
            string currentYear = DateTime.Now.Year.ToString();
            string strPrevYear = Convert.ToString(DateTime.Now.Year - 1);

            List<SelectListItem> UpcomingActivityList = new List<SelectListItem>();
            foreach (var Planyear in PlanYearList)
            {
                var checkYear = UpcomingActivityList.Where(a => a.Text == Planyear).Select(a => a.Text).FirstOrDefault();
                if (checkYear == null)
                {
                    UpcomingActivityList.Add(new SelectListItem { Text = Planyear, Value = Planyear, Selected = false });
                }
            }
            //// Fetch the pervious year and future year list and insert into the list object
            var yearlistPrevious = activePlan.Where(plan => plan.Year != currentYear && Convert.ToInt32(plan.Year) < DateTime.Now.Year).Select(plan => plan.Year).Distinct().OrderBy(year => year).ToList();
            yearlistPrevious.ForEach(year => UpcomingActivityList.Add(new SelectListItem { Text = year, Value = year, Selected = false }));




            string strThisQuarter = Enums.UpcomingActivities.thisquarter.ToString(), strThisMonth = Enums.UpcomingActivities.thismonth.ToString(),
                                    quartText = Enums.UpcomingActivitiesValues[strThisQuarter].ToString(), monthText = Enums.UpcomingActivitiesValues[strThisMonth].ToString();
            string MinYear = string.Empty;
            string MaxYear = string.Empty;
            //// If active plan dosen't have any current plan at that time we have to remove this month and thisquater option
            if (activePlan != null && activePlan.Any())
            {
                //// Add current year into the list

                UpcomingActivityList.Add(new SelectListItem { Text = quartText, Value = strThisQuarter, Selected = false });
                UpcomingActivityList.Add(new SelectListItem { Text = monthText, Value = strThisMonth, Selected = false });

                if (lstCampaign.Count > 0)
                {
                    MinYear = Convert.ToString(lstCampaign.Select(a => a.StartDate.Year).Min());
                    MaxYear = Convert.ToString(lstCampaign.Select(a => a.EndDate.Year).Max());
                }
                else
                {
                    MinYear = Convert.ToString(PlanYearList.Select(a => Convert.ToInt32(a)).Min());
                    MaxYear = Convert.ToString(PlanYearList.Select(a => Convert.ToInt32(a)).Max());
                }
                foreach (var camp in lstCampaign)
                {
                    int campPlanYear = Convert.ToInt32(camp.Plan.Year);
                    int campStYear = camp.StartDate.Year;
                    int campEdYear = camp.EndDate.Year;
                    int campYearDiffer = campEdYear - campStYear;
                    string EndYear = camp.EndDate.Year.ToString();
                    string StartYear = camp.StartDate.Year.ToString();
                    if (Convert.ToInt32(MinYear) < campStYear)
                    {
                        MinYear = Convert.ToString(campStYear);
                    }
                    if (Convert.ToInt32(MaxYear) < campEdYear)
                    {
                        MaxYear = Convert.ToString(campEdYear);
                    }
                    if (campStYear != campEdYear)
                    {
                        //var checkYear = UpcomingActivityList.Where(a => a.Text == EndYear).Select(a => a.Text).FirstOrDefault();
                        //var checkFromTo = UpcomingActivityList.Where(a => a.Text == currentYear + "-" + EndYear).Select(a => a.Text).FirstOrDefault();
                        int yearDiff = Convert.ToInt32(EndYear) - Convert.ToInt32(campStYear);
                        for (int i = 1; i <= yearDiff; i++)
                        {
                            var checkEndYear = UpcomingActivityList.Where(a => a.Text == EndYear).Select(a => a.Text).FirstOrDefault();
                            if (checkEndYear == null)
                            {
                                UpcomingActivityList.Add(new SelectListItem { Text = EndYear, Value = EndYear, Selected = false });
                            }
                            var checkStartYear = UpcomingActivityList.Where(a => a.Text == StartYear).Select(a => a.Text).FirstOrDefault();
                            if (checkStartYear == null)
                            {
                                UpcomingActivityList.Add(new SelectListItem { Text = StartYear, Value = StartYear, Selected = false });
                            }
                        }
                        if (campYearDiffer > 0)
                        {
                            var checkFromTo = UpcomingActivityList.Where(a => a.Text == campStYear + "-" + campEdYear).Select(a => a.Text).FirstOrDefault();
                            if (checkFromTo == null)
                            {
                                UpcomingActivityList.Add(new SelectListItem { Text = campStYear + "-" + campEdYear, Value = campStYear + "-" + campEdYear, Selected = false });
                            }
                        }
                    }
                    else
                    {
                        var checkMinYear = UpcomingActivityList.Where(a => a.Text == MinYear).Select(a => a.Text).FirstOrDefault();
                        if (checkMinYear == null)
                        {
                            UpcomingActivityList.Add(new SelectListItem { Text = MinYear, Value = MinYear, Selected = false });
                        }
                        var checkMaxYear = UpcomingActivityList.Where(a => a.Text == MaxYear).Select(a => a.Text).FirstOrDefault();
                        if (checkMaxYear == null)
                        {
                            UpcomingActivityList.Add(new SelectListItem { Text = MaxYear, Value = MaxYear, Selected = false });
                        }
                    }
                    if (campPlanYear != camp.StartDate.Year || campPlanYear != camp.EndDate.Year)
                    {
                        int campyear = campPlanYear != camp.StartDate.Year ? camp.StartDate.Year : camp.EndDate.Year;
                        var checkFromTo = UpcomingActivityList.Where(a => a.Text == campPlanYear + "-" + campyear).Select(a => a.Text).FirstOrDefault();
                        if (checkFromTo == null)
                        {
                            UpcomingActivityList.Add(new SelectListItem { Text = campPlanYear + "-" + campyear, Value = campPlanYear + "-" + campyear, Selected = false });
                        }
                    }
                }
                // UpcomingActivityList.Add(new SelectListItem { Text = currentYear, Value = currentYear, Selected = true });
            }
            List<string> upcometext = UpcomingActivityList.Select(a => a.Text).ToList();
            bool currentyearfirst = false;
            bool currentyearsecond = false;
            bool notcurrentyearmax = false;
            int upcomingloop = 1;
            UpcomingActivityList.ForEach(a =>
            {
                var item = a.Text.Split('-');
                if (item.Length > 1 && item[0] == currentYear && !currentyearfirst)
                {
                    a.Selected = true;
                    currentyearfirst = true;
                }
                else if (item.Length > 1)
                {
                    if (item[1] == currentYear && !currentyearfirst)
                    {
                        a.Selected = true;
                        currentyearsecond = true;
                    }
                    else if (!currentyearfirst && !currentyearsecond && !notcurrentyearmax && item[1] == MaxYear)
                    {
                        a.Selected = true;
                        notcurrentyearmax = true;
                    }
                }
                else if (!currentyearfirst && !currentyearsecond && !notcurrentyearmax && upcomingloop == UpcomingActivityList.Count)
                {
                    UpcomingActivityList.Where(act => act.Text == (upcometext.Contains(currentYear) ? currentYear : MinYear)).ToList().ForEach(act => act.Selected = true);
                }
                upcomingloop += 1;
            });

            #region "Verify filter selected year has current year and previous year"
            List<string> lstFilterYear = new List<string>();
            if (!string.IsNullOrEmpty(fltrYears))
            {
                lstFilterYear = fltrYears.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

                // Verify whether current & previous year plan selected in filter & exist in lstFiterYear or not.
                if ((lstFilterYear.Any(yr => yr.Equals(currentYear)) && lstFilterYear.Any(yr => yr.Equals(strPrevYear))) && (PlanYearList.Any(yr => yr.Equals(currentYear)) && PlanYearList.Any(yr => yr.Equals(strPrevYear))))
                {
                    //Set selected value 'false' for all items in UpcomingActivityList.
                    UpcomingActivityList.ForEach(item => item.Selected = false);
                    string strTimeFrame = strPrevYear + "-" + currentYear;

                    // Remove old currentYear-PrevYear Timeframe value from UpcomingActivityList.
                    if (UpcomingActivityList.Any(year => year.Text.Equals(strTimeFrame)))
                    {
                        var multiTimeFrame = UpcomingActivityList.Where(year => year.Text.Equals(strTimeFrame)).FirstOrDefault();
                        UpcomingActivityList.Remove(multiTimeFrame);
                    }
                    // Set currentyear - PrevYear range selected to True.
                    UpcomingActivityList.Add(new SelectListItem { Text = strTimeFrame, Value = strTimeFrame, Selected = true });
                }
            }
            #endregion

            UpcomingActivityList = UpcomingActivityList.GroupBy(a => a.Text).Select(x => x.First()).ToList();

            //var yearlistAfter = activePlan.Where(plan => plan.Year != currentYear && Convert.ToInt32(plan.Year) > DateTime.Now.Year).Select(plan => plan.Year).Distinct().OrderBy(year => year).ToList();
            //yearlistAfter.ForEach(year => UpcomingActivityList.Add(new SelectListItem { Text = year, Value = year, Selected = false }));
            UpcomingActivityList = UpcomingActivityList.Distinct().ToList();
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
            Sessions.PlanPlanIds = filterplanIds;
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
                lstAllowedEntityIds = TacticUserList.Select(a => a.PlanTacticId).ToList();
            }
            //List<TacticTypeModel> objTacticType = TacticUserList.GroupBy(pc => new { title = pc.TacticType.Title, id = pc.TacticTypeId }).Select(pc => new TacticTypeModel
            //{
            //    Title = pc.Key.title,
            //    TacticTypeId = pc.Key.id,
            //    Number = pc.Count()
            //}).OrderBy(TacticType => TacticType.Title, new AlphaNumericComparer()).ToList();
            List<TacticTypeModel> objTacticType = objSp.GetTacticTypeList(string.Join(",", lstAllowedEntityIds));
            return objTacticType;
        }


        //Added by Komal rawal for #1283
        public async Task<JsonResult> GetTacticTypeListForFilter(string PlanId)
        {
            try
            {

                List<int> lstPlanIds = string.IsNullOrWhiteSpace(PlanId) ? new List<int>() : PlanId.Split(',').Select(plan => int.Parse(plan)).ToList();
                Sessions.PlanPlanIds = lstPlanIds;
                // Add By Nishant Sheth
                // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                DataSet dsPlanCampProgTac = new DataSet();
                dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(PlanId);
                objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);
                //var tacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && lstPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactic => tactic).ToList();

                var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);
                // Add By Nishant Sheth
                // Desc :: Set tatcilist for original db/modal format
                var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);

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
            var NewListOfViews = new List<Plan_UserSavedViews>();// Add By Nishant Sheth to resolved the default view issue with update result
            //Modified for #1750 by Komal Rawal

            //Modified By komal Rawal for #1959 to handle last viewed data in session
            var listofsavedviews = new List<Plan_UserSavedViews>();
            if (Sessions.PlanUserSavedViews == null || isLoadPreset == true)
            {
                listofsavedviews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).Select(view => view).ToList();
                Common.PlanUserSavedViews = listofsavedviews;
            }
            else
            {

                Common.PlanUserSavedViews = Sessions.PlanUserSavedViews;
                listofsavedviews = Common.PlanUserSavedViews;
            }
            //ENd
            // Add By Nishant Sheth #1915


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


                newList = newList.GroupBy(g => g.Name).Select(x => x.FirstOrDefault()).OrderBy(g => g.Name).ToList();

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
                    newListSearch = newListSearch.GroupBy(g => g.Name).Select(x => x.FirstOrDefault()).OrderBy(g => g.Name).ToList();
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
                    if (Sessions.PlanUserSavedViews == null)
                    {
                        // Add By Nishant Sheth to resolved the default view issue with update result
                        NewListOfViews = listofsavedviews.Where(view => view.IsDefaultPreset == true).ToList();
                        if (NewListOfViews.Count == 0)
                        {
                            listofsavedviews = listofsavedviews.Where(view => view.ViewName == null).ToList();
                        }
                        else
                        {
                            listofsavedviews = NewListOfViews;
                        }

                    }
                    else
                    {
                        listofsavedviews = Sessions.PlanUserSavedViews.Where(view => view.ViewName == null).ToList();
                    }

                    // End By Nishant Sheth
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

                var YearLabel = Enums.FilterLabel.Year.ToString();
                var LastSetOfYears = new List<string>();

                var SetOfYears = listofsavedviews.Where(view => view.FilterName == YearLabel).Select(View => View.FilterValues).ToList();
                if (SetOfYears.Count > 0)
                {
                    if (SetOfYears.FirstOrDefault() != null)
                    {
                        LastSetOfYears = SetOfYears.FirstOrDefault().Split(',').ToList();
                    }
                    //else
                    //{
                    //    LastSetOfTacticType = null;
                    //}

                }

                var LastSetofCustomField = listofsavedviews.Where(view => view.FilterName.Contains("CF")).Select(view => new { ID = view.FilterName, Value = view.FilterValues }).ToList();

                Sessions.FilterPresetName = null;
                return Json(new { StatusNAmes = LastSetOfStatus, Customfields = LastSetofCustomField, OwnerNames = LastSetOfOwners, TTList = LastSetOfTacticType, Years = LastSetOfYears }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Added By: KOmal Rawal.
        /// Action to save last accessed data
        /// </summary>
        /// <param name="PlanId">Plan Id and filters</param>
        public JsonResult SaveLastSetofViews(string planId, string customFieldIds, string ownerIds, string TacticTypeid, string StatusIds, string ViewName, string SelectedYears, string ParentCustomFieldsIds)
        {


            Sessions.PlanUserSavedViews = null;  //Added By komal Rawal for #1959 to handle last viewed data in session
            //Modified by Rahul Shah to remove ternary operator. 
            List<int> planIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(planId))
            {
                planIds = planId.Split(',').Select(plan => int.Parse(plan)).ToList();
            }
            //List<int> planIds = string.IsNullOrWhiteSpace(planId) ? new List<int>() : planId.Split(',').Select(plan => int.Parse(plan)).ToList();
            List<Plan> ListofPlans = objDbMrpEntities.Plans.Where(p => planIds.Contains(p.PlanId)).ToList();
            string planPublishedStatus = Enums.PlanStatusValues.FirstOrDefault(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            planIds = ListofPlans.Where(plan => plan.Status.Equals(planPublishedStatus)).Select(plan => plan.PlanId).ToList();
            // Added By Komal Rawal for #1750 to default users to their last view
            List<Plan_UserSavedViews> NewCustomFieldData = new List<Plan_UserSavedViews>();
            //NewCustomFieldData = new List<Plan_UserSavedViews>();
            List<string> tempFilterValues = new List<string>();
            #region "Remove previous records by userid"
            //var prevCustomFieldList = objDbMrpEntities.Plan_UserSavedViews.Where(custmfield => custmfield.Userid == Sessions.User.UserId).ToList();
            var prevCustomFieldList = Common.PlanUserSavedViews;// Add By Nishant Sheth #1915
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
            //Modified By Komal Rawal on 12-02-16 created a function InsertLastViewedUserData to decrease same lines of code.

            if (planIds.Count != 0)
            {
                InsertLastViewedUserData(planId, ViewName, Enums.FilterLabel.Plan.ToString(), NewCustomFieldData);

            }
            //if (ownerIds != null && ownerIds != "") // Commented By Nishant Sheth Desc:: To resolve owner filter issue
            {
                InsertLastViewedUserData(ownerIds, ViewName, Enums.FilterLabel.Owner.ToString(), NewCustomFieldData);

            }
            //if (TacticTypeid != null && TacticTypeid != "") //Commented by Rahul Shah for PL #1952.
            {
                //if(TacticTypeid == "0")
                //{
                //    TacticTypeid = null;
                //}
                InsertLastViewedUserData(TacticTypeid, ViewName, Enums.FilterLabel.TacticType.ToString(), NewCustomFieldData);

            }
            if (StatusIds != "AddActual" && StatusIds != "Report")
            {
                //if (StatusIds != null && StatusIds != "")  //Commented by Rahul Shah for PL #1952.
                {
                    InsertLastViewedUserData(StatusIds, ViewName, Enums.FilterLabel.Status.ToString(), NewCustomFieldData);
                }
            }


            if (SelectedYears != null && SelectedYears != "")
            {
                InsertLastViewedUserData(SelectedYears, ViewName, Enums.FilterLabel.Year.ToString(), NewCustomFieldData);

            }
            //Modified by Rahul shah on 18/02/2016 to optimize code.
            string[] filterValues = { };
            string[] filteredCustomFields = { };
            string PrefixCustom = "CF_";
            string FilterNameTemp = "";
            string Previousval = "";
            var PreviousValue = "";
            string CustomOptionvalue = string.Empty;
            var ExistingFieldlist = new Plan_UserSavedViews();
            if (customFieldIds != "")
            {
                //Added and Commented by Rahul to remove ternory operator and add if..else condition                
                if (string.IsNullOrWhiteSpace(customFieldIds))
                {
                    filteredCustomFields = null;
                }
                else
                {
                    filteredCustomFields = customFieldIds.Split(',');
                }
                //string[] filteredCustomFields = string.IsNullOrWhiteSpace(customFieldIds) ? null : customFieldIds.Split(',');
                if (filteredCustomFields != null)
                {
                    Plan_UserSavedViews objFilterValues = new Plan_UserSavedViews();
                    List<Plan_UserSavedViews> listLineitem = Common.PlanUserSavedViews;// Add By Nishant Sheth #1915
                    Plan_UserSavedViews objLineitem = new Plan_UserSavedViews();
                    foreach (string customField in filteredCustomFields)
                    {
                        filterValues = customField.Split('_');
                        if (filterValues.Count() > 1)
                        {
                            CustomOptionvalue = filterValues[1];
                        }
                        if (filterValues.Count() > 0)
                        {
                            PreviousValue = PrefixCustom + filterValues[0].ToString();
                            ExistingFieldlist = listLineitem.Where(pcpobjw => pcpobjw.FilterName.Equals(PreviousValue)).FirstOrDefault();
                            //Added and Commented by Rahul to remove ternary operator and add if..else condition
                            if (string.IsNullOrEmpty(FilterNameTemp))
                            {
                                if (ExistingFieldlist != null)
                                {
                                    FilterNameTemp += ExistingFieldlist.FilterValues;
                                }
                                else
                                {
                                    FilterNameTemp += "";
                                }
                                //FilterNameTemp += (ExistingFieldlist != null ? ExistingFieldlist.FilterValues : "");
                            }
                            else
                            {
                                FilterNameTemp += "";
                            }

                            //FilterNameTemp += string.IsNullOrEmpty(FilterNameTemp) ? (ExistingFieldlist != null ? ExistingFieldlist.FilterValues : "") : "";
                            if (FilterNameTemp == PreviousValue && !string.IsNullOrEmpty(CustomOptionvalue))
                            {
                                Previousval += ',' + CustomOptionvalue;
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
                                objFilterValues.FilterName = PrefixCustom + filterValues[0];
                                objFilterValues.FilterValues = CustomOptionvalue;
                                objFilterValues.Userid = Sessions.User.UserId;
                                objFilterValues.LastModifiedDate = DateTime.Now;
                                objFilterValues.IsDefaultPreset = false;
                                FilterNameTemp = "";
                                FilterNameTemp += PrefixCustom + filterValues[0].ToString();
                                Previousval = "";
                                Previousval = CustomOptionvalue;
                                objDbMrpEntities.Plan_UserSavedViews.Add(objFilterValues);
                            }

                        }
                        NewCustomFieldData.Add(objFilterValues);
                    }


                }
            }
            else
            {   //Added by Rahul Shah for PL #1952. to save custom filter name when all deselect
                if (string.IsNullOrWhiteSpace(ParentCustomFieldsIds))
                {
                    filteredCustomFields = null;
                }
                else
                {
                    filteredCustomFields = ParentCustomFieldsIds.Split(',');
                }
                if (filteredCustomFields != null)
                {
                    Plan_UserSavedViews objFilterValues = new Plan_UserSavedViews();
                    foreach (string customField in filteredCustomFields)
                    {
                        objFilterValues = new Plan_UserSavedViews();
                        objFilterValues.ViewName = null;
                        if (ViewName != null && ViewName != "")
                        {
                            objFilterValues.ViewName = ViewName;
                        }
                        objFilterValues.FilterName = PrefixCustom + customField;
                        objFilterValues.FilterValues = "";
                        objFilterValues.Userid = Sessions.User.UserId;
                        objFilterValues.LastModifiedDate = DateTime.Now;
                        objFilterValues.IsDefaultPreset = false;
                        objDbMrpEntities.Plan_UserSavedViews.Add(objFilterValues);
                        NewCustomFieldData.Add(objFilterValues);
                    }
                }
            }

            //if (StatusIds != null && StatusIds != "")//Commented by Rahul Shah for PL #1952.
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
                    //Modified By Komal Rawal for #2138 remove concurrency issue
                    var ids = prevCustomFieldList.Select(t => t.Id).ToList();
                    string ListOfPreviousIDs = null;
                    if (ids.Count > 0)
                    {
                        ListOfPreviousIDs = string.Join(",", ids);
                    }

                    objDbMrpEntities.DeleteLastViewedData(Sessions.User.UserId.ToString(), ListOfPreviousIDs); //Sp to delete last viewed data before inserting new one.
                    objDbMrpEntities.SaveChanges();
                    //End
                }
                else
                {
                    if (prevCustomFieldList.Count == 0)
                    {
                        objDbMrpEntities.SaveChanges();
                    }
                }
            }
            // Add By Nishant Sheth #1915
            Common.PlanUserSavedViews = objDbMrpEntities.Plan_UserSavedViews.Where(custmfield => custmfield.Userid == Sessions.User.UserId).ToList();
            #endregion
            //End
            Sessions.PlanUserSavedViews = Common.PlanUserSavedViews;  //Added By komal Rawal for #1959 to handle last viewed data in session
            return Json(new { isSuccess = true, ViewName = ViewName }, JsonRequestBehavior.AllowGet);
        }

        //Added By Komal Rawal on 12-02-16 for code optimazation.
        //Desc : Function to insert data into Plan_UserSaved Views table for filters in left pane.
        private void InsertLastViewedUserData(string Ids, string ViewName, string FilterName, List<Plan_UserSavedViews> NewCustomFieldData)
        {
            Plan_UserSavedViews objFilterValues = new Plan_UserSavedViews();
            objFilterValues.ViewName = null;
            if (ViewName != null && ViewName != "")
            {
                objFilterValues.ViewName = ViewName;
            }
            objFilterValues.FilterName = FilterName;
            objFilterValues.FilterValues = Ids;
            objFilterValues.Userid = Sessions.User.UserId;
            objFilterValues.LastModifiedDate = DateTime.Now;
            objFilterValues.IsDefaultPreset = false;
            NewCustomFieldData.Add(objFilterValues);
            objDbMrpEntities.Entry(objFilterValues).State = EntityState.Added;
        }
        //End

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
            //var ListOfUserViews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            var ListOfUserViews = Common.PlanUserSavedViews;// Add By Nishant Sheth #1915
            var ResetFlagList = ListOfUserViews.Where(view => view.IsDefaultPreset == true).ToList();
            ResetFlagList.ForEach(s => { s.IsDefaultPreset = false; objDbMrpEntities.Entry(s).State = EntityState.Modified; });// Modified By Nishant Sheth #1915
            if (!string.IsNullOrEmpty(PresetName))
            {
                ListOfUserViews = ListOfUserViews.Where(name => name.ViewName == PresetName).ToList();
                ListOfUserViews.ForEach(s => { s.IsDefaultPreset = true; objDbMrpEntities.Entry(s).State = EntityState.Modified; });// Modified By Nishant Sheth #1915

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

                //var lstViewID = objDbMrpEntities.Plan_UserSavedViews.Where(x => x.Userid == Sessions.User.UserId && x.ViewName == PresetName).ToList();
                var lstViewID = Common.PlanUserSavedViews.Where(x => x.Userid == Sessions.User.UserId && x.ViewName == PresetName).ToList();// Add By Nishant Sheth #1915
                if (lstViewID != null)
                {
                    foreach (var item in lstViewID)
                    {
                        Plan_UserSavedViews planToDelete = lstViewID.Where(x => x.Id == item.Id).FirstOrDefault(); // Modified By Nishant Sheth #1915
                        //objDbMrpEntities.Plan_UserSavedViews.Remove(planToDelete);
                        objDbMrpEntities.Entry(planToDelete).State = EntityState.Deleted; // Add By Nishant Sheth #1915
                    }
                    objDbMrpEntities.SaveChanges();
                    Common.PlanUserSavedViews = objDbMrpEntities.Plan_UserSavedViews.Where(x => x.Userid == Sessions.User.UserId).ToList();// Add By Nishant Sheth #1915
                    return Json(new { isSuccess = true, msg = "Preset " + PresetName + " deleted successfully" }, JsonRequestBehavior.AllowGet); //Modified by Maitri Gandhi on 28/4/2016 for #2136
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
            string[] ListYear = strParam.Split('-');// Add By Nishant Sheth
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
            List<ActivityChart> lstActivityChart = new List<ActivityChart>();
            if (strParam.Contains("-"))
            {
                //List<ActivityChart> lstActivityChartyears = new List<ActivityChart>();
                lstActivityChart = getmultipleyearActivityChartHoneyComb(strParam, TactIds, objPlan_Campaign_Program_Tactic1);// Modified By Nishant Sheth #1876
                //return Json(new { lstchart = lstActivityChartyears.ToList() }, JsonRequestBehavior.AllowGet);
            }
            // else

            isNumeric = int.TryParse(ListYear[0], out Planyear);
            if (isNumeric)
            {
                planYear = Convert.ToString(Planyear);
            }
            else
            {
                planYear = DateTime.Now.Year.ToString();
            }

            Common.GetPlanGanttStartEndDate(planYear, strParam, ref CalendarStartDate, ref CalendarEndDate);
            var objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic =>
                                      TacticIds.Contains(tactic.PlanTacticId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate))).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate, isdelete = tactic.IsDeleted }).ToList();

            objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(tactic => tactic.isdelete.Equals(false)).ToList();

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
                            thismonthdifferenceItem.RemoveRange(12, thismonthdifferenceItem.Count - 12);
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
            if (!strParam.Contains("-"))
            {
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
            }
            // await Task.Delay(1);


            return Json(new { lstchart = lstActivityChart.ToList(), TotalMql = MQLs, TotalCost = tactcost, TotalCount = tactcount, }, JsonRequestBehavior.AllowGet);

        }

        private List<ActivityChart> getmultipleyearActivityChartHoneyComb(string strParam, string TactIds, List<Plan_Campaign_Program_Tactic> objPlan_Campaign_Program_Tactic) // Modified By Nishant Sheth #1876 to avoid db trips
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
                //var objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => 
                //  TacticIds.Contains(tactic.PlanTacticId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate))).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate,isdelete=tactic.IsDeleted }).ToList();

                //objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(tactic => tactic.isdelete.Equals(false)).ToList();

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


                                    //if (monthNo == 1)
                                    //{
                                    //    monthArray[0] = monthArray[0] + 1;
                                    //}
                                    //else
                                    //{
                                    //    monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                    //}
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetOwnerName(Guid UserGuid)
        {
            var OwnerName = "";
            if (lstUsers.Count == 0 || lstUsers == null)
            {
                objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId).ForEach(u => lstUsers.Add(u.UserId, u));
            }
            if (UserGuid != Guid.Empty)
            {
                if (lstUsers.ContainsKey(UserGuid))
                {
                    var userName = lstUsers[UserGuid];
                    if (userName != null)
                    {
                        OwnerName = string.Format("{0} {1}", Convert.ToString(userName.FirstName), Convert.ToString(userName.LastName));
                    }
                }
            }
            return Convert.ToString(OwnerName);
        }

        #endregion

        #region ROI Packaging
        /// <summary>
        /// Create new ROI package
        /// </summary>
        /// <param name="AnchorTacticId"></param>
        /// <param name="PromotionTacticIds"></param>
        /// <returns></returns>
        public JsonResult AddROIPackageDetails(int AnchorTacticId, string PromotionTacticIds = "")
        {
            string[] arrPromoTacticIds = null;
            ROI_PackageDetail newPackage = null;

            try
            {
                if (!string.IsNullOrEmpty(PromotionTacticIds))
                {
                    arrPromoTacticIds = PromotionTacticIds.Split(',');
                }

                // Delete existing tactics from package
                List<ROI_PackageDetail> lstPkgDelete = new List<ROI_PackageDetail>();
                lstPkgDelete = objDbMrpEntities.ROI_PackageDetail.Where(p => p.AnchorTacticID == AnchorTacticId).ToList();
                if (lstPkgDelete != null && lstPkgDelete.Count > 0)
                {
                    lstPkgDelete.ForEach(x => objDbMrpEntities.Entry(x).State = EntityState.Deleted);
                    objDbMrpEntities.SaveChanges();
                }

                // Create new package 
                foreach (var tacticId in arrPromoTacticIds)
                {
                    newPackage = new ROI_PackageDetail();
                    newPackage.AnchorTacticID = AnchorTacticId;
                    newPackage.PlanTacticId = Convert.ToInt32(tacticId);
                    newPackage.CreatedDate = DateTime.Now;
                    newPackage.CreatedBy = Sessions.User.UserId;
                    objDbMrpEntities.ROI_PackageDetail.Add(newPackage);
                }
                objDbMrpEntities.Entry(newPackage).State = EntityState.Added;
                objDbMrpEntities.SaveChanges();

            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { data = "Success" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Delete tactics from package or
        /// Delete entire package
        /// </summary>
        /// <param name="AnchorTacticId"></param>
        /// <param name="IsPromotion"></param>
        /// <returns></returns>
        public JsonResult UnpackageTactics(int AnchorTacticId, bool IsPromotion = false)
        {
            List<int> remainItems = new List<int>();

            try
            {
                List<ROI_PackageDetail> lstPkgDelete = new List<ROI_PackageDetail>();
                if (IsPromotion)
                {
                    lstPkgDelete = objDbMrpEntities.ROI_PackageDetail.Where(p => p.PlanTacticId == AnchorTacticId).ToList();
                    if (lstPkgDelete != null && lstPkgDelete.Count > 0)
                    {
                        int AncTacId = lstPkgDelete.FirstOrDefault().AnchorTacticID;
                        remainItems = objDbMrpEntities.ROI_PackageDetail.Where(x => x.AnchorTacticID == AncTacId && x.PlanTacticId != AnchorTacticId).Select(y => y.PlanTacticId).ToList();
                    }
                }
                else
                {
                    lstPkgDelete = objDbMrpEntities.ROI_PackageDetail.Where(p => p.AnchorTacticID == AnchorTacticId).ToList();
                }
                lstPkgDelete.ForEach(x => objDbMrpEntities.Entry(x).State = EntityState.Deleted);

                objDbMrpEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { remainItems = remainItems }, JsonRequestBehavior.AllowGet);
        }

        #endregion



        public void GetCacheValue()
        {

            // Add By Nishant Sheth
            // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
            var PlanId = string.Join(",", Sessions.PlanPlanIds);
            DataSet dsPlanCampProgTac = new DataSet();
            dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(PlanId);
            objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

            List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
            objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

            var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
            objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

            var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
            objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);
            //var tacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && lstPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactic => tactic).ToList();

            var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
            objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);
            // Add By Nishant Sheth
            // Desc :: Set tatcilist for original db/modal format
            var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
            objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);
        }


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
        public DateTime maxDate { get; set; }
        public List<Custom_Plan_Campaign_Program_Tactic> CustomTacticList { get; set; }
    }

    public class PlanMinMaxDate
    {
        public int PlanId { get; set; }
        public int CustomFieldId { get; set; }
        public DateTime minDate { get; set; }
        public DateTime maxDate { get; set; }
        public DateTime minEffectiveDate { get; set; }
    }

}
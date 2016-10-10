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
using System.Web;
using System.Data;
using System.Threading.Tasks;
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
        Dictionary<int, User> lstUsers = new Dictionary<int, BDSService.User>();
        List<TacticType> TacticTypeList = new List<TacticType>();
        List<ROI_PackageDetail> ROIPackageAnchorTacticList = new List<ROI_PackageDetail>();
        private BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
        CacheObject objCache = new CacheObject(); // Add By Nishant Sheth // Desc:: For get values from cache
        StoredProcedure objSp = new StoredProcedure();// Add By Nishant Sheth // Desc:: For get values with storedprocedure

        #endregion

        //public ActionResult IndexNewDesign() // Added by Bhumika for new HTML #2621 Remove when task finished 
        //{
        //    return View();
        //}

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

        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Home, int currentPlanId = 0, int planTacticId = 0, int planCampaignId = 0, int planProgramId = 0, bool isImprovement = false, bool isGridView = true, int planLineItemId = 0, bool IsPlanSelector = false, int PreviousPlanID = 0, bool IsRequest = false, bool ShowPopup = false, bool IsBudgetView = false)
        {
            Guid AppId = Sessions.User.UserApplicationId.Where(o => o.ApplicationTitle == Enums.ApplicationCode.MRP.ToString()).Select(o => o.ApplicationId).FirstOrDefault();
            Guid RoleId = Sessions.User.UserApplicationId.Where(o => o.ApplicationTitle == Enums.ApplicationCode.MRP.ToString()).Select(o => o.RoleIdApplicationWise).FirstOrDefault();
            Sessions.User.RoleId = RoleId;
            Sessions.AppMenus = objBDSServiceClient.GetMenu(AppId, Sessions.User.RoleId);
            Sessions.RolePermission = objBDSServiceClient.GetPermission(AppId, Sessions.User.RoleId);
            if (Sessions.AppMenus != null)
            {
                var isAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ModelCreateEdit);
                var item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.Model.ToString().ToUpper());
                if (item != null && !isAuthorized)
                {
                    Sessions.AppMenus.Remove(item);
                }

                isAuthorized = (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostImprovementTacticCreateEdit) ||
                    AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostBestInClassNumberEdit));
                item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.Boost.ToString().ToUpper());
                if (item != null && !isAuthorized)
                {
                    Sessions.AppMenus.Remove(item);
                }

                isAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ReportView);
                item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.Report.ToString().ToUpper());
                if (item != null && !isAuthorized)
                {
                    Sessions.AppMenus.Remove(item);
                }

                isAuthorized = (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit) ||
                      AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView) ||
                      AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit) ||
                      AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView));

                item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.MarketingBudget.ToString().ToUpper());
                if (item != null && !isAuthorized)
                {
                    Sessions.AppMenus.Remove(item);
                }

                isAuthorized = Sessions.AppMenus.Select(x => x.Code.ToLower()).Contains(Enums.ActiveMenu.Plan.ToString().ToLower());
                item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.Finance.ToString().ToUpper());
                if (item != null && !isAuthorized)
                {
                    Sessions.AppMenus.Remove(item);
                }
            }
            ViewBag.IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            ViewBag.IsTacticActualsAddEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);
            bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
            bool IsPlanEditable = false;
            bool isPublished = false;
            ViewBag.IsPlanSelector = IsPlanSelector;
            ViewBag.PreviousPlanID = PreviousPlanID;
            ViewBag.IsRequest = IsRequest;
            Dictionary<string, string> ColorCodelist = objDbMrpEntities.EntityTypeColors.ToDictionary(e => e.EntityType.ToLower(), e => e.ColorCode);
            ColorCodelist = objDbMrpEntities.EntityTypeColors.ToDictionary(e => e.EntityType.ToLower(), e => e.ColorCode);
            var TacticColor = ColorCodelist[Enums.EntityType.Tactic.ToString().ToLower()];
            ViewBag.TacticTaskColor = TacticColor;
            ViewBag.ActiveMenu = activeMenu;
            ViewBag.ShowInspectForPlanTacticId = planTacticId;
            ViewBag.ShowInspectForPlanCampaignId = planCampaignId;
            ViewBag.ShowInspectForPlanProgramId = planProgramId;
            ViewBag.IsImprovement = isImprovement;
            ViewBag.GridView = isGridView;
            ViewBag.BudgetView = IsBudgetView;
            ViewBag.ShowInspectForPlanLineItemId = planLineItemId;
            if (currentPlanId > 0)
            {
                var lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.ID);
                var objPlan = objDbMrpEntities.Plans.FirstOrDefault(_plan => _plan.PlanId == currentPlanId);

                if (objPlan.CreatedBy == Sessions.User.ID)
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

                Plan Plan = objPlan;
                isPublished = Plan.Status.Equals(Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString());
             

            }
            ViewBag.IsPlanEditable = IsPlanEditable;
            ViewBag.IsPublished = isPublished;
            ViewBag.currentPlanId = currentPlanId;
            ViewBag.RedirectType = Enums.InspectPopupRequestedModules.Index.ToString();
            //set value to show inspect popup for url sent in email 
            if (currentPlanId > 0 && ShowPopup)
            {
                ViewBag.ShowInspectPopup = true;
                currentPlanId = InspectPopupSharedLinkValidation(currentPlanId, planCampaignId, planProgramId, planTacticId, isImprovement, planLineItemId);
            }
            else if (currentPlanId <= 0 && (planTacticId > 0 || planCampaignId > 0 || planProgramId > 0))
            {
                ViewBag.ShowInspectPopup = false;
                ViewBag.ShowInspectPopupErrorMessage = Common.objCached.InvalidURLForInspectPopup.ToString();
            }
            else if (currentPlanId <= 0)
            {
                ViewBag.ShowInspectPopup = false;
                ViewBag.ShowInspectPopupErrorMessage = Common.objCached.InvalidURLForInspectPopup.ToString();
            }
          
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


            ViewBag.IsPlanEditable = IsPlanEditable;


            return View();

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

        //Add by Komal Rawal on 12/09/2016
        //Desc : To get header values.

        public async Task<JsonResult> GetActivityDistributionchart(string planid, string strtimeframe, string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "", bool IsGridView = false)
        {

            List<int> filteredPlanIds = new List<int>();
            string planYear = string.Empty;
            int Planyear;
            bool isNumeric = false;
            List<int> campplanid = new List<int>();
            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;
            isNumeric = int.TryParse(strtimeframe, out Planyear);
            if (isNumeric)
            {
                planYear = Convert.ToString(Planyear);
            }
            else
            {
                planYear = DateTime.Now.Year.ToString();
            }

            if (string.IsNullOrEmpty(strtimeframe))
            {
                List<Plan> lstPlans = new List<Plan>();
                lstPlans = Common.GetPlan();
                int planId;
                Int32.TryParse(planid, out planId);
                Plan objPlan = lstPlans.Where(x => x.PlanId == planId).FirstOrDefault();
                if (objPlan != null)
                {
                    planYear = objPlan.Year;
                    strtimeframe = planYear;
                }

            }

            //// Set start and end date for calender
            Common.GetPlanGanttStartEndDate(planYear, strtimeframe, ref CalendarStartDate, ref CalendarEndDate);
            DataSet dsPlanCampProgTac = new DataSet();
            dsPlanCampProgTac = (DataSet)objCache.Returncache(Enums.CacheObject.dsPlanCampProgTac.ToString());
            if (dsPlanCampProgTac == null)
            {
                dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(planid);
            }
            List<Plan> planData = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
            List<Plan_Campaign> lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]);

            bool IsMultiYearPlan = false;

            List<int> planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(plan => int.Parse(plan)).ToList();

            List<int> planList = planData.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && plan.Year == planYear).Select(a => a.PlanId).ToList();

            if (planList.Count == 0)
            {
                campplanid = lstCampaign.Where(camp => !(camp.StartDate > CalendarEndDate || camp.EndDate < CalendarStartDate) && planIds.Contains(camp.PlanId)).Select(a => a.PlanId).Distinct().ToList();
            }
            filteredPlanIds = planData.Where(plan => plan.IsDeleted == false &&
                campplanid.Count > 0 ? campplanid.Contains(plan.PlanId) : planIds.Contains(plan.PlanId)).ToList().Select(plan => plan.PlanId).ToList();




            //// Get planyear of the selected Plan
            if (strtimeframe.Contains("-") || IsMultiYearPlan)
            {

                List<ActivityChart> lstActivityChartyears = new List<ActivityChart>();
                lstActivityChartyears = GetmultipleyearActivityChartData(strtimeframe, planid, CustomFieldId, OwnerIds, TacticTypeids, StatusIds);

                return Json(new { lstchart = lstActivityChartyears.ToList(), strparam = strtimeframe }, JsonRequestBehavior.AllowGet);
            }

            var objPlan_Campaign_Program_Tactic = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]).Where(tactic =>
                                                   campplanid.Count > 0 ? campplanid.Contains(tactic.PlanId) : filteredPlanIds.Contains(tactic.PlanId) &&
                                                   tactic.EndDate > CalendarStartDate &&
                                                   tactic.StartDate < CalendarEndDate &&
                                                   tactic.IsDeleted == false).
                                                   Select(tactic => new
                                                   {
                                                       PlanTacticId = tactic.PlanTacticId,
                                                       CreatedBy = tactic.CreatedBy,
                                                       TacticTypeId = tactic.TacticTypeId,
                                                       Status = tactic.Status,
                                                       StartDate = tactic.StartDate,
                                                       EndDate = tactic.EndDate,
                                                       isdelete = tactic.IsDeleted
                                                   }).ToList();

            objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(tactic => tactic.isdelete.Equals(false)).ToList();

            List<string> lstFilteredCustomFieldOptionIds = new List<string>();
            List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
            List<int> lstTacticIds = new List<int>();

            //// Owner filter criteria.
            List<int> filterOwner = string.IsNullOrWhiteSpace(OwnerIds) ? new List<int>() : OwnerIds.Split(',').Select(owner => Int32.Parse(owner)).ToList();

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

            if ((filterOwner.Count > 0 || filterTacticType.Count > 0 || filterStatus.Count > 0 || filteredCustomFields.Count > 0))
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


            //// Prepare an array of month as per selected dropdown paramter
            int[] monthArray = new int[12];

            if (objPlan_Campaign_Program_Tactic != null && objPlan_Campaign_Program_Tactic.Count() > 0)
            {
                IEnumerable<string> differenceItems;
                int year = 0;
                if (strtimeframe != null && isNumeric)
                {
                    year = Planyear;
                }

                int currentMonth = DateTime.Now.Month, monthNo = 0;
                DateTime startDate, endDate;

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
                    else if (strtimeframe.Equals(Enums.UpcomingActivities.thismonth.ToString(), StringComparison.OrdinalIgnoreCase))
                    {

                        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                        differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM-yyyy"));

                        List<string> thismonthdifferenceItem = new List<string>();

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

                            }
                        }
                    }
                    else if (strtimeframe.Equals(Enums.UpcomingActivities.thisquarter.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (startDate.Year == System.DateTime.Now.Year || endDate.Year == System.DateTime.Now.Year)
                        {
                            if (currentMonth == 1 || currentMonth == 2 || currentMonth == 3)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q1), startDate, endDate, monthArray);

                            }
                            else if (currentMonth == 4 || currentMonth == 5 || currentMonth == 6)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q2), startDate, endDate, monthArray);

                            }
                            else if (currentMonth == 7 || currentMonth == 8 || currentMonth == 9)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q3), startDate, endDate, monthArray);

                            }
                            else if (currentMonth == 10 || currentMonth == 11 || currentMonth == 12)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q4), startDate, endDate, monthArray);

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

            return Json(new { lstchart = lstActivityChart.ToList(), strtimeframe = strtimeframe }, JsonRequestBehavior.AllowGet); //Modified BY Komal rawal for #1929 proper Hud chart and count
        }

        private List<ActivityChart> GetmultipleyearActivityChartData(string strtimeframe, string planid, string CustomFieldId, string OwnerIds, string TacticTypeids, string StatusIds)
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
            if (strtimeframe.Contains("-"))
            {

                planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(plan => int.Parse(plan)).ToList();

                planData = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                List<Plan_Campaign> lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]);
                string[] multipleyear = strtimeframe.Split('-');
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


                    List<int> planList = planData.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && plan.Year == planYear).Select(a => a.PlanId).ToList();
                    if (planList.Count == 0)
                    {
                        campplanid = lstCampaign.Where(camp => !(camp.StartDate > CalendarEndDate || camp.EndDate < CalendarStartDate) && planIds.Contains(camp.PlanId)).Select(a => a.PlanId).Distinct().ToList();
                    }
                    filteredPlanIds = planData.Where(plan => plan.IsDeleted == false &&
                     campplanid.Count > 0 ? campplanid.Contains(plan.PlanId) : planIds.Contains(plan.PlanId)).ToList().Select(plan => plan.PlanId).ToList();


                    var objPlan_Campaign_Program_Tactic = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]).Where(tactic =>
                                                 campplanid.Count > 0 ? campplanid.Contains(tactic.PlanId) : filteredPlanIds.Contains(tactic.PlanId)
                                                 && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate)) && tactic.IsDeleted == false)
                                                 .Select(tactic => new
                                                 {
                                                     PlanTacticId = tactic.PlanTacticId,
                                                     CreatedBy = tactic.CreatedBy,
                                                     TacticTypeId = tactic.TacticTypeId,
                                                     Status = tactic.Status,
                                                     StartDate = tactic.StartDate,
                                                     EndDate = tactic.EndDate,
                                                     isdelete = tactic.IsDeleted
                                                 }).ToList();

                    objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(tactic => tactic.isdelete.Equals(false)).ToList();

                    List<string> lstFilteredCustomFieldOptionIds = new List<string>();
                    List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                    List<int> lstTacticIds = new List<int>();

                    // Owner filter criteria.
                    List<int> filterOwner = string.IsNullOrWhiteSpace(OwnerIds) ? new List<int>() : OwnerIds.Split(',').Select(owner => Int32.Parse(owner)).ToList();

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

                    if ((filterOwner.Count > 0 || filterTacticType.Count > 0 || filterStatus.Count > 0 || filteredCustomFields.Count > 0))
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public JsonResult GetActualTactic(int status, string tacticTypeId, string customFieldId, string ownerId, int PlanId, int UserId = 0)
        {
            if (UserId != 0)
            {
                if (Sessions.User.ID != UserId)
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnLoginURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            //// Start - Added by Sohel Pathan on 22/01/2015 for PL ticket #1144
            //// Get TacticTypes selected  in search filter
            List<int> filteredTacticTypeIds = string.IsNullOrWhiteSpace(tacticTypeId) ? new List<int>() : tacticTypeId.Split(',').Select(tacticType => int.Parse(tacticType)).ToList();

            //// Owner filter criteria.
            List<int> filteredOwner = string.IsNullOrWhiteSpace(ownerId) ? new List<int>() : ownerId.Split(',').Select(owner => Convert.ToInt32(owner)).ToList();
            //// End - Added by Sohel Pathan on 22/01/2015 for PL ticket #1144
            string stageMQL = Enums.Stage.MQL.ToString();

            // Add By Nishant Sheth
            // Desc :: To get performance regarding #2111 add stagelist into cache memory
            CacheObject dataCache = new CacheObject();
            List<Stage> stageList = dataCache.Returncache(Enums.CacheObject.StageList.ToString()) as List<Stage>;
            if (stageList == null)
            {
                stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == Sessions.User.CID && stage.IsDeleted == false).Select(stage => stage).ToList();
            }
            dataCache.AddCache(Enums.CacheObject.StageList.ToString(), stageList);
            int levelMQL = stageList.FirstOrDefault(s => s.ClientId.Equals(Sessions.User.CID) && s.IsDeleted == false && s.Code.Equals(stageMQL)).Level.Value;

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

                List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.ID, Sessions.User.CID, TacticIds, false);
                TacticList = TacticList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.ID).Select(tactic => tactic).ToList();
            }

            var lstPlanTacticActual = (from tacticActual in objDbMrpEntities.Plan_Campaign_Program_Tactic_Actual
                                       where TacticIds.Contains(tacticActual.PlanTacticId)
                                       select new { tacticActual.PlanTacticId, tacticActual.CreatedBy, tacticActual.CreatedDate }).GroupBy(tacticActual => tacticActual.PlanTacticId).Select(tacticActual => tacticActual.FirstOrDefault());

            List<int> userListId = new List<int>();
            userListId = (from tacticActual in lstPlanTacticActual select tacticActual.CreatedBy).ToList<int>();

            //// Added BY Bhavesh, Calculate MQL at runtime #376
            List<Plan_Tactic_Values> MQLTacticList = Common.GetMQLValueTacticList(TacticList);
            List<ProjectedRevenueClass> tacticList = Common.ProjectedRevenueCalculateList(TacticList);
            List<ProjectedRevenueClass> tacticListCW = Common.ProjectedRevenueCalculateList(TacticList, true);
            var lstModifiedTactic = TacticList.Where(tactic => tactic.ModifiedDate != null).Select(tactic => tactic).ToList();
            foreach (var tactic in lstModifiedTactic)
            {
                userListId.Add(tactic.ModifiedBy);
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
                List<int> lstSubordinatesIds = new List<int>();
                if (IsTacticAllowForSubordinates)
                {
                    lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.ID);
                }
                //// End - Added by Sohel Pathan on 03/02/2015 for PL ticket #1090

                List<int> lstViewEditEntities = Common.GetEditableTacticList(Sessions.User.ID, Sessions.User.CID, TacticIds, true);

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

                    IsTacticEditable = ((tactic.CreatedBy.Equals(Sessions.User.ID) || lstSubordinatesIds.Contains(tactic.CreatedBy)) ? (lstViewEditEntities.Contains(tactic.PlanTacticId)) : false),
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
                    roiProjected = tacticActual.costProjected == 0 ? 0 : (((tacticActual.revenueProjected - tacticActual.costProjected) / tacticActual.costProjected) * 100),// Modified By Nishant Sheth // #2376 Change the formula for ROI Projected
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
                OwnerId = owner.ID,
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

                if (dsPlanCampProgTac == null)
                {
                    dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(PlanId);
                }
                List<Plan_Campaign> campaignList = new List<Plan_Campaign>();
                if (dsPlanCampProgTac != null && dsPlanCampProgTac.Tables[1] != null)
                {
                    campaignList = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]);
                }

                objCache.AddCache(Enums.CacheObject.Campaign.ToString(), campaignList);

                var campaignListids = campaignList.Select(campaign => campaign.PlanCampaignId).ToList();

                List<Plan_Campaign_Program> programList = new List<Plan_Campaign_Program>();
                if (dsPlanCampProgTac != null && dsPlanCampProgTac.Tables[2] != null)
                {
                    programList = Common.GetSpProgramList(dsPlanCampProgTac.Tables[2]);
                }

                objCache.AddCache(Enums.CacheObject.Program.ToString(), programList);

                var programListids = programList.Select(program => program.PlanProgramId).ToList();
                //List<int> programownerids = programList.Select(program => program.CreatedBy).Distinct().ToList();
                // Add By Nishant Sheth
                // Get Records from cache memory
                List<Custom_Plan_Campaign_Program_Tactic> customtacticList = (List<Custom_Plan_Campaign_Program_Tactic>)objCache.Returncache(Enums.CacheObject.CustomTactic.ToString());
                if (customtacticList == null || customtacticList.Count == 0)
                {
                    customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                }
                objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);
                List<Plan_Campaign_Program_Tactic> tacticList = (List<Plan_Campaign_Program_Tactic>)objCache.Returncache(Enums.CacheObject.Tactic.ToString());
                if (tacticList == null || tacticList.Count == 0)
                {
                    tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                }
                objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);
                //var tacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && programListids.Contains(tactic.PlanProgramId)).Select(tactic => tactic).ToList();
                //List<int> planTacticIds = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
                //List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.ID, Sessions.User.CID, planTacticIds, false);
                //Added by Rahul Shah on 06/01/2016 for PL#1854.
                string section = Enums.Section.Tactic.ToString();


                var customfield = objDbMrpEntities.CustomFields.Where(customField => customField.EntityType == section && customField.ClientId == Sessions.User.CID && customField.IsDeleted == false).ToList();
                objCache.AddCache(Enums.CacheObject.CustomField.ToString(), customfield);
                var customfieldidlist = customfield.Select(c => c.CustomFieldId).ToList();
                var lstAllTacticCustomFieldEntitiesanony = objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => customfieldidlist.Contains(customFieldEntity.CustomFieldId))
                                                                                       .Select(customFieldEntity => new CacheCustomField { EntityId = customFieldEntity.EntityId, CustomFieldId = customFieldEntity.CustomFieldId, Value = customFieldEntity.Value, CreatedBy = customFieldEntity.CreatedBy, CustomFieldEntityId = customFieldEntity.CustomFieldEntityId }).Distinct().ToList();

                //List<CustomField_Entity> lstAllTacticCustomFieldEntitiesanony = objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => customfieldidlist.Contains(customFieldEntity.CustomFieldId))
                //                                                                       .Select(customFieldEntity => new CustomField_Entity { EntityId = customFieldEntity.EntityId, CustomFieldId = customFieldEntity.CustomFieldId, Value = customFieldEntity.Value }).Distinct().ToList();
                //var lstAllTacticCustomFieldEntitiesanony = objSp.GetCustomFieldEntityList(string.Join(",", customfieldidlist));
                objCache.AddCache(Enums.CacheObject.CustomFieldEntity.ToString(), lstAllTacticCustomFieldEntitiesanony);

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

                    List<int> AllowedEntityIds = Common.GetViewableTacticList(Sessions.User.ID, Sessions.User.CID, planTacticIds, false, customfieldlist);
                    if (AllowedEntityIds.Count > 0)
                    {
                        lstAllowedEntityIds.AddRange(AllowedEntityIds);
                    }

                }

                //Added by Nishant to bring the owner name in the list even if they dont own any tactic
                var LoggedInUser = new OwnerModel
                {
                    OwnerId = Sessions.User.ID,
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
                //     TacticUserList = TacticUserList.Where(tactic => status.Contains(tactic.Status) || ((tactic.CreatedBy == Sessions.User.ID && !ViewBy.Equals(GanttTabs.Request.ToString())) ? statusCD.Contains(tactic.Status) : false)).Distinct().ToList();
                //TacticUserList = TacticUserList.Where(tactic => !ViewBy.Equals(GanttTabs.Request.ToString())).Distinct().ToList();

                if (TacticUserList.Count > 0)
                {
                    //// Custom Restrictions applied
                    TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.ID).ToList();
                }
                var useridslist = TacticUserList.Select(tactic => tactic.CreatedBy).Distinct().ToList();
                var individuals = bdsUserRepository.GetMultipleTeamMemberNameByApplicationIdEx(useridslist, Sessions.ApplicationId); //PL 1569 Dashrath Prajapati

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
                    TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.ID).ToList();
                }
                var useridslist = TacticUserList.Select(tactic => tactic.CreatedBy).Distinct().ToList();
                var individuals = bdsUserRepository.GetMultipleTeamMemberNameByApplicationIdEx(useridslist, Sessions.ApplicationId); //PL 1569 Dashrath Prajapati

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
        public ActionResult CheckUserId(int UserId)
        {
            if (Sessions.User.ID == UserId)
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
                    activePlan = objDbMrpEntities.Plans.Where(plan => plan.IsActive.Equals(true) && plan.IsDeleted == false && plan.Status == publishedPlanStatus && plan.Model.ClientId == Sessions.User.CID)
                                                            .Select(plan => new { plan.PlanId, plan.Title })
                                                            .ToList().Where(plan => !string.IsNullOrEmpty(plan.Title)).OrderBy(plan => plan.Title, new AlphaNumericComparer()).ToList();
                }
                else
                {
                    activePlan = objDbMrpEntities.Plans.Where(plan => plan.IsActive.Equals(true) && plan.IsDeleted == false && plan.Model.ClientId == Sessions.User.CID)
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
            var lstCustomField = objDbMrpEntities.CustomFields.Where(customField => customField.ClientId == Sessions.User.CID && customField.IsDeleted.Equals(false) &&
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
                var userCustomRestrictionList = Common.GetUserCustomRestrictionsList(Sessions.User.ID, true);   //// Modified by Sohel Pathan on 15/01/2015 for PL ticket #1139

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
        /// modified by Mitesh vaishnav add flag to check request come from calender view or budget view
        public JsonResult BindUpcomingActivitesValues(string planids, string fltrYears, bool IsBudgetGrid = false)
        {
            //// Fetch the list of Upcoming Activity
            List<SelectListItem> objUpcomingActivity = UpComingActivity(planids, fltrYears, IsBudgetGrid);
            objUpcomingActivity = objUpcomingActivity.Where(activity => !string.IsNullOrEmpty(activity.Text)).OrderBy(activity => activity.Text, new AlphaNumericComparer()).ToList();
            return Json(objUpcomingActivity.ToList(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Function to process and fetch the Upcoming Activity  
        /// </summary>
        /// <param name="PlanIds">comma sepreated string plan id(s)</param>
        /// <returns>List fo SelectListItem of Upcoming activity</returns>
        public List<SelectListItem> UpComingActivity(string PlanIds, string fltrYears, bool IsBudgetGrid = false)
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



            //Add this year (quarterly) and this year (monthly) option to timeframe data
            string strThisQuarter = Enums.UpcomingActivities.ThisYearQuaterly.ToString();
            string strThisMonth = Enums.UpcomingActivities.ThisYearMonthly.ToString();
            string quartText = Enums.UpcomingActivitiesValues[strThisQuarter].ToString();
            string monthText = Enums.UpcomingActivitiesValues[strThisMonth].ToString();

            //for calander add this month and this quarter option instead of this year (quarterly) and this year (monthly)
            if (!IsBudgetGrid)
            {
                strThisQuarter = Enums.UpcomingActivities.thisquarter.ToString();
                strThisMonth = Enums.UpcomingActivities.thismonth.ToString();
                quartText = Enums.UpcomingActivitiesValues[strThisQuarter].ToString();
                monthText = Enums.UpcomingActivitiesValues[strThisMonth].ToString();
            }

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
        private bool InspectPopupSharedLinkValidationForCustomRestriction(int planCampaignId, int planProgramId, int planTacticId, bool isImprovement, int currentPlanId, int planLineItemId = 0)
        {
            bool isValidEntity = false;

            if (planTacticId > 0 && isImprovement.Equals(false))
            {
                List<int> AllowedTacticIds = new List<int>();
                AllowedTacticIds = Common.GetViewableTacticList(Sessions.User.ID, Sessions.User.CID, new List<int>() { planTacticId }, false);

                var objTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == planTacticId && tactic.IsDeleted == false
                                                                    && (AllowedTacticIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.ID))
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
            else if (currentPlanId > 0)
            {
                var objPlan = objDbMrpEntities.Plans.Where(plan => plan.PlanId == currentPlanId && plan.IsDeleted == false
                                                                ).Select(plan => plan.PlanId);

                if (objPlan.Count() != 0)
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
                TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.ID).ToList();
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
                objCache.AddCache(Enums.CacheObject.PlanTacticListforpackageing.ToString(), customtacticList);  //Added by Komal Rawal for #2358 show all tactics in package even if they are not filtered
                // Add By Nishant Sheth
                // Desc :: Set tatcilist for original db/modal format
                var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);

                List<int> planTacticIds = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
                List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.ID, Sessions.User.CID, planTacticIds, false);
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

        public PermissionModel GetPermission(int id, string section, List<int> lstSubordinatesIds, string InspectPopupMode = "")
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
                        if (objPlan_Campaign_Program_Tactic.CreatedBy.Equals(Sessions.User.ID) || lstSubordinatesIds.Contains(objPlan_Campaign_Program_Tactic.CreatedBy))
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
                        if (objPlan_Campaign_Program.CreatedBy.Equals(Sessions.User.ID) || lstSubordinatesIds.Contains(objPlan_Campaign_Program.CreatedBy))
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
                        if (objPlan_Campaign.CreatedBy.Equals(Sessions.User.ID) || lstSubordinatesIds.Contains(objPlan_Campaign.CreatedBy))
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
                        if (objPlan_Campaign_Program_Tactic_LineItem.CreatedBy.Equals(Sessions.User.ID))
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
                        if (objplan.CreatedBy.Equals(Sessions.User.ID))
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
                listofsavedviews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.ID).Select(view => view).ToList();
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
            //var prevCustomFieldList = objDbMrpEntities.Plan_UserSavedViews.Where(custmfield => custmfield.Userid == Sessions.User.ID).ToList();
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
                                objFilterValues.Userid = Sessions.User.ID;
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
                        objFilterValues.Userid = Sessions.User.ID;
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

                    objDbMrpEntities.DeleteLastViewedData(Sessions.User.ID, ListOfPreviousIDs); //Sp to delete last viewed data before inserting new one.
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
            Common.PlanUserSavedViews = objDbMrpEntities.Plan_UserSavedViews.Where(custmfield => custmfield.Userid == Sessions.User.ID).ToList();
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
            objFilterValues.Userid = Sessions.User.ID;
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
            //var ListOfUserViews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.ID).ToList();
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

                //var lstViewID = objDbMrpEntities.Plan_UserSavedViews.Where(x => x.Userid == Sessions.User.ID && x.ViewName == PresetName).ToList();
                var lstViewID = Common.PlanUserSavedViews.Where(x => x.Userid == Sessions.User.ID && x.ViewName == PresetName).ToList();// Add By Nishant Sheth #1915
                if (lstViewID != null)
                {
                    foreach (var item in lstViewID)
                    {
                        Plan_UserSavedViews planToDelete = lstViewID.Where(x => x.Id == item.Id).FirstOrDefault(); // Modified By Nishant Sheth #1915
                        //objDbMrpEntities.Plan_UserSavedViews.Remove(planToDelete);
                        objDbMrpEntities.Entry(planToDelete).State = EntityState.Deleted; // Add By Nishant Sheth #1915
                    }
                    objDbMrpEntities.SaveChanges();
                    Common.PlanUserSavedViews = objDbMrpEntities.Plan_UserSavedViews.Where(x => x.Userid == Sessions.User.ID).ToList();// Add By Nishant Sheth #1915
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

        #region GetOwnerName and TacticTypeName and AssetValue
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
        public string GetOwnerName(int userId)
        {
            var OwnerName = "";
            if (lstUsers.Count == 0 || lstUsers == null)
            {
                objBDSServiceClient.GetUserListByClientIdEx(Sessions.User.CID).ForEach(u => lstUsers.Add(u.ID, u));
            }
            if (userId != 0)
            {
                if (lstUsers.ContainsKey(userId))
                {
                    var userName = lstUsers[userId];
                    if (userName != null)
                    {
                        OwnerName = string.Format("{0} {1}", Convert.ToString(userName.FirstName), Convert.ToString(userName.LastName));
                    }
                }
            }
            return Convert.ToString(OwnerName);
        }

        /// <summary>
        /// Added By: Komal Rawal.
        /// Action to get all Assetvalue for tactics
        /// Date : 14-07-16
        /// </summary>
        public string GettactictypeAssetValue(int TacticTypeID)
        {
            if (TacticTypeList.Count == 0 || TacticTypeList == null)
            {
                TacticTypeList = objDbMrpEntities.TacticTypes.Where(tt => tt.TacticTypeId == TacticTypeID && tt.IsDeleted == false).Select(tt => tt).ToList();
            }
            var AssetTacticType = TacticTypeList.Where(tt => tt.TacticTypeId == TacticTypeID).Select(tt => tt.AssetType).FirstOrDefault();

            return AssetTacticType;
        }

        /// <summary>
        /// Added By: Komal Rawal.
        /// Action to get AnchorTacticId of each package
        /// Date : 14-07-16
        /// </summary>
        public int AnchorTacticInPackage(int TacticId)
        {
            //bool IsAnchor = false;
            if (ROIPackageAnchorTacticList.Count == 0 || ROIPackageAnchorTacticList == null)
            {
                ROIPackageAnchorTacticList = objDbMrpEntities.ROI_PackageDetail.ToList();
            }
            var AnchorTacticid = ROIPackageAnchorTacticList.Where(p => p.PlanTacticId == TacticId).Select(pkg => pkg.AnchorTacticID).FirstOrDefault();

            return AnchorTacticid;
        }

        /// <summary>
        /// Added By: Komal Rawal.
        /// Action to get tactic ids of each package
        /// Date : 14-07-16
        /// </summary>
        public string PackageTacticIds(int TacticId)
        {
            if (ROIPackageAnchorTacticList.Count == 0 || ROIPackageAnchorTacticList == null)
            {
                ROIPackageAnchorTacticList = objDbMrpEntities.ROI_PackageDetail.ToList();
            }
            var PackageTacticIDs = ROIPackageAnchorTacticList.Where(list => list.AnchorTacticID == TacticId).Select(list => list.PlanTacticId).ToList();
            var CommaSeparatedPackageTacticIDs = String.Join(",", PackageTacticIDs);
            return CommaSeparatedPackageTacticIDs;
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
            Dictionary<int, int> planTacAnchorTac = new Dictionary<int, int>();
            bool IsUpdatePackage = false;
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
                    IsUpdatePackage = true;

                    // Update AnchorTacticId of tactics in cache
                    lstPkgDelete.ForEach(pkg => planTacAnchorTac.Add(pkg.PlanTacticId, 0));
                    Common.UpdateAnchorTacticInCache(planTacAnchorTac);

                    lstPkgDelete.ForEach(x => objDbMrpEntities.Entry(x).State = EntityState.Deleted);
                    objDbMrpEntities.SaveChanges();
                }

                if (arrPromoTacticIds != null)
                {
                    // Create new package 
                    foreach (var tacticId in arrPromoTacticIds)
                    {
                        newPackage = new ROI_PackageDetail();
                        newPackage.AnchorTacticID = AnchorTacticId;
                        newPackage.PlanTacticId = Convert.ToInt32(tacticId);
                        newPackage.CreatedDate = DateTime.Now;
                        newPackage.CreatedBy = Sessions.User.ID;
                        objDbMrpEntities.ROI_PackageDetail.Add(newPackage);
                    }
                    objDbMrpEntities.Entry(newPackage).State = EntityState.Added;
                    objDbMrpEntities.SaveChanges();

                    // Update AnchorTacticId of tactics in cache
                    planTacAnchorTac = new Dictionary<int, int>();
                    arrPromoTacticIds.Select(int.Parse).ToList().ForEach(planTacId => planTacAnchorTac.Add(planTacId, AnchorTacticId));
                    Common.UpdateAnchorTacticInCache(planTacAnchorTac);
                }

            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { data = "Error" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { data = "Success", IsUpdatePackage = IsUpdatePackage }, JsonRequestBehavior.AllowGet);
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

                // Update AnchorTacticId of tactics in cache
                Dictionary<int, int> planTacAnchorTac = new Dictionary<int, int>();
                lstPkgDelete.ForEach(pkg => planTacAnchorTac.Add(pkg.PlanTacticId, 0));
                Common.UpdateAnchorTacticInCache(planTacAnchorTac);

                lstPkgDelete.ForEach(x => objDbMrpEntities.Entry(x).State = EntityState.Deleted);
                objDbMrpEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { remainItems = remainItems }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added BY : Komal Rawal
        /// Date : 18-07-2016
        /// Show tactic in package even if they are not filtered
        /// Ticket # 2358
        /// </summary>
        public JsonResult GetPackageTacticDetails(string viewBy, string TacticIds = "", string TacticTaskColor = "", bool IsGridView = false)
        {
            var PlanTacticListforpackageing = (List<Custom_Plan_Campaign_Program_Tactic>)objCache.Returncache(Enums.CacheObject.PlanTacticListforpackageing.ToString());
            var Listofdata = new object();
            if (PlanTacticListforpackageing == null || PlanTacticListforpackageing.Count == 0)
            {
                var PlanId = string.Join(",", Sessions.PlanPlanIds);
                DataSet dsPlanCampProgTac = new DataSet();
                dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(PlanId);

                var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                objCache.AddCache(Enums.CacheObject.PlanTacticListforpackageing.ToString(), customtacticList);
                PlanTacticListforpackageing = customtacticList;
            }

            if (viewBy.Equals(PlanGanttTypes.Tactic.ToString(), StringComparison.OrdinalIgnoreCase))
            {

                Listofdata = PlanTacticListforpackageing.Where(id => TacticIds.Contains(id.PlanTacticId.ToString())).Select(tactic => new
                {
                    TacticId = tactic.PlanTacticId,
                    TaskId = string.Format("L{0}_C{1}_P{2}_T{3}", tactic.PlanId, tactic.PlanCampaignId, tactic.PlanProgramId, tactic.PlanTacticId),
                    Title = tactic.Title,
                    TacticTypeValue = tactic.TacticTypeTtile != "" ? tactic.TacticTypeTtile : "null",
                    ColorCode = TacticTaskColor,
                    OwnerName = GetOwnerName(tactic.CreatedBy),
                    ROITacticType = tactic.AssetType,
                    CalendarEntityType = "Tactic",
                    AnchorTacticId = tactic.AnchorTacticId,
                    CsvId = "Tactic_" + tactic.PlanTacticId,
                });

            }
            else if (viewBy.Equals(PlanGanttTypes.Stage.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                Listofdata = PlanTacticListforpackageing.Where(id => TacticIds.Contains(id.PlanTacticId.ToString())).Select(tactic => new
                {
                    TacticId = tactic.PlanTacticId,
                    TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tactic.StageId, tactic.PlanId, tactic.PlanCampaignId, tactic.PlanProgramId, tactic.PlanTacticId),
                    Title = tactic.Title,
                    TacticTypeValue = tactic.TacticTypeTtile != "" ? tactic.TacticTypeTtile : "null",
                    ColorCode = TacticTaskColor,
                    OwnerName = GetOwnerName(tactic.CreatedBy),
                    ROITacticType = tactic.AssetType,
                    CalendarEntityType = "Tactic",
                    AnchorTacticId = tactic.AnchorTacticId,
                    CsvId = "Tactic_" + tactic.PlanTacticId,
                });


            }
            else if (viewBy.Equals(PlanGanttTypes.Status.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                Listofdata = PlanTacticListforpackageing.Where(id => TacticIds.Contains(id.PlanTacticId.ToString())).Select(tactic => new
                {
                    TacticId = tactic.PlanTacticId,
                    TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tactic.Status, tactic.PlanId, tactic.PlanCampaignId, tactic.PlanProgramId, tactic.PlanTacticId),
                    Title = tactic.Title,
                    TacticTypeValue = tactic.TacticTypeTtile != "" ? tactic.TacticTypeTtile : "null",
                    ColorCode = TacticTaskColor,
                    OwnerName = GetOwnerName(tactic.CreatedBy),
                    ROITacticType = tactic.AssetType,
                    CalendarEntityType = "Tactic",
                    AnchorTacticId = tactic.AnchorTacticId,
                    CsvId = "Tactic_" + tactic.PlanTacticId,
                });


            }
            else if (viewBy.Equals(Enums.DictPlanGanttTypes[PlanGanttTypes.ROIPackage.ToString()].ToString(), StringComparison.OrdinalIgnoreCase))
            {
                Listofdata = PlanTacticListforpackageing.Where(id => TacticIds.Contains(id.PlanTacticId.ToString())).Select(tactic => new
                {
                    TacticId = tactic.PlanTacticId,
                    TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tactic.AnchorTacticId, tactic.PlanId, tactic.PlanCampaignId, tactic.PlanProgramId, tactic.PlanTacticId),
                    Title = tactic.Title,
                    TacticTypeValue = tactic.TacticTypeTtile != "" ? tactic.TacticTypeTtile : "null",
                    ColorCode = TacticTaskColor,
                    OwnerName = GetOwnerName(tactic.CreatedBy),
                    ROITacticType = tactic.AssetType,
                    CalendarEntityType = "Tactic",
                    AnchorTacticId = tactic.AnchorTacticId,
                    CsvId = "Tactic_" + tactic.PlanTacticId,
                });


            }
            else
            {
                Listofdata = PlanTacticListforpackageing.Where(id => TacticIds.Contains(id.PlanTacticId.ToString())).Select(tactic => new
                {
                    TacticId = tactic.PlanTacticId,
                    TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tactic.PlanTacticId, tactic.PlanId, tactic.PlanCampaignId, tactic.PlanProgramId, tactic.PlanTacticId),
                    Title = tactic.Title,
                    TacticTypeValue = tactic.TacticTypeTtile != "" ? tactic.TacticTypeTtile : "null",
                    ColorCode = TacticTaskColor,
                    OwnerName = GetOwnerName(tactic.CreatedBy),
                    ROITacticType = tactic.AssetType,
                    CalendarEntityType = "Tactic",
                    AnchorTacticId = tactic.AnchorTacticId,
                    CsvId = "Tactic_" + tactic.PlanTacticId,
                });

            }

            return Json(new { Listofdata = Listofdata }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region method for getting goal value.
        public JsonResult GetGoalValues(string planids)
        {
            var MQL = Enums.PlanGoalType.MQL.ToString();
            var INQ = Enums.PlanGoalType.INQ.ToString();
            var Revenue = Enums.PlanGoalType.Revenue.ToString();
            var CW = Enums.Stage.CW.ToString();
            string MQLLabel = string.Empty;
            string INQLabel = string.Empty;
            double MQLValue = 0;
            double INQValue = 0;
            double RevenueValue = 0;
            string RevenueLabel = string.Empty;
            string CWLabel = string.Empty;
            double CWValue = 0;
            List<GoalValueModel> GoalValueListResult = objSp.spgetgoalvalues(planids);

            MQLLabel = GoalValueListResult.Where(list => list.StageCode == MQL).Select(list => list.Title).FirstOrDefault();
            MQLValue = GoalValueListResult.Where(list => list.StageCode == MQL).Select(list => list.Value).FirstOrDefault();
            INQLabel = GoalValueListResult.Where(list => list.StageCode == INQ).Select(list => list.Title).FirstOrDefault();
            INQValue = GoalValueListResult.Where(list => list.StageCode == INQ).Select(list => list.Value).FirstOrDefault();
            CWLabel = GoalValueListResult.Where(list => list.StageCode == CW).Select(list => list.Title).FirstOrDefault();
            CWValue = GoalValueListResult.Where(list => list.StageCode == CW).Select(list => list.Value).FirstOrDefault();
            RevenueLabel = GoalValueListResult.Where(list => list.StageCode == Revenue).Select(list => list.Title).FirstOrDefault();
            RevenueValue = GoalValueListResult.Where(list => list.StageCode == Revenue).Select(list => list.Value).FirstOrDefault();

            return Json(new { MQLLabel = MQLLabel, MQLValue = MQLValue, INQLabel = INQLabel, INQValue = INQValue, CWLabel = CWLabel, CWValue = CWValue, RevenueLabel = RevenueLabel, RevenueValue = RevenueValue }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region"Plan Calendar related functions"
        /// <summary>
        /// Created by: Viral
        /// Created On: 09/19/2016
        /// Desc: Return Calendar PartialView 
        /// </summary>
        /// <returns> Calendar PartialView Result</returns>
        [HttpPost]
        public PartialViewResult LoadPlanCalendar()
        {
            return PartialView("_PlanCalendar");
        }
        /// <summary>
        /// Created by: Viral
        /// Created On: 09/19/2016
        /// Desc: Get Calendar Model data to bind Calendar 
        /// </summary>
        /// <returns> Return Calendar Json Result</returns>
        [HttpPost]
        public JsonResult GetCalendarData(string planIds, string ownerIds, string tactictypeIds, string statusIds, string customFieldIds, string timeframe, string viewBy)
        {
            #region "Declare local variables"
            string planYear = "";
            Services.IGrid objGrid = new Services.Grid();
            List<calendarDataModel> resultData = new List<calendarDataModel>();
            #endregion

            try
            {
                // if viewby value is empty then set default to 'Tactic'
                if (string.IsNullOrEmpty(viewBy))
                    viewBy = PlanGanttTypes.Tactic.ToString();

                // Get Calendar data through SP.
                resultData = objGrid.GetPlanCalendarData(planIds, ownerIds, tactictypeIds, statusIds, customFieldIds, timeframe, planYear, viewBy);

                // Set Owner Name and permission for each required entity.
                resultData = objGrid.SetOwnerNameAndPermission(resultData);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);  // Log error in Elmah.
            }

            var jsonResult = Json(new { data = resultData }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }


        #endregion

        #region "ViewBy related functions"
        /// <summary>
        /// Created by: Viral
        /// Created On: 09/26/2016
        /// Desc: Get Viewby list data. 
        /// </summary>
        /// <returns>Return Viewby list data.  </returns>
        public JsonResult GetViewBylistData(string planids)
        {
            List<ViewByModel> lstViewBy = new List<ViewByModel>();
            try
            {
                lstViewBy = (new StoredProcedure()).spViewByDropDownList(planids);  // Get ViewBy options by PlanIds.
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(lstViewBy, JsonRequestBehavior.AllowGet);
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
            objCache.AddCache(Enums.CacheObject.PlanTacticListforpackageing.ToString(), customtacticList);  //Added by Komal Rawal for #2358 show all tactics in package even if they are not filtered
            // Add By Nishant Sheth
            // Desc :: Set tatcilist for original db/modal format
            var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
            objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);
        }


    }
}
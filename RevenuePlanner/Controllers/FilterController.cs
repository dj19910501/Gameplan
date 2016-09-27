using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Controllers
{
    public class FilterController : CommonController
    {
        #region Variables
        IFilter objCommonFilter = new RevenuePlanner.Services.Filter();
        #endregion

        /// <summary>
        /// Filter Index Page
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="activeMenu">activeMenu</param>
        /// <param name="currentPlanId">currentPlanId</param>
        /// <returns>returns partial view of Filter</returns>
        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Home, List<int> currentPlanId = null)
        {
            #region Declare Variables
            HomePlanModel PlanModel = new Models.HomePlanModel();
            #endregion

            List<SelectListItem> LstYear = new List<SelectListItem>();
            List<Plan_UserSavedViews> PlanUserSavedViews = Sessions.PlanUserSavedViews;
            PlanModel = objCommonFilter.GetFilterData(currentPlanId, Sessions.User.ID, Sessions.User.CID, Sessions.PlanPlanIds, Sessions.FilterPresetName, ref PlanUserSavedViews, ref LstYear);
            Sessions.PlanUserSavedViews = PlanUserSavedViews;
            if (PlanModel.lstPlanId.Count() > 0)
            {
                Sessions.PlanPlanIds = PlanModel.lstPlanId;
            }
            ViewBag.ViewYear = LstYear;
            ViewBag.activeMenu = activeMenu;

            return PartialView("Filters", PlanModel);
        }

        /// <summary>
        /// Get TacticType List For Filter
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="PlanId">PlanId</param>
        /// <returns>returns Json as TacticType List</returns>
        public JsonResult GetTacticTypeListForFilter(string PlanId)
        {
            try
            {
                List<int> lstPlanIds = string.IsNullOrWhiteSpace(PlanId) ? new List<int>() : PlanId.Split(',').Select(plan => int.Parse(plan)).ToList();
                Sessions.PlanPlanIds = lstPlanIds;
                List<TacticTypeModel> lstTacticType = objCommonFilter.GetTacticTypeListForFilter(PlanId, Sessions.User.ID, Sessions.User.CID);
                return Json(new { isSuccess = true, TacticTypelist = lstTacticType }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Last Set Of Views
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="PresetName">PresetName</param>
        /// <param name="isLoadPreset">isLoadPreset</param>
        /// <returns>returns Json as Last Set Of Views</returns>
        public ActionResult LastSetOfViews(string PresetName = "", Boolean isLoadPreset = false)
        {
            string StatusLabel = Convert.ToString(Enums.FilterLabel.Status);
            List<string> LastSetOfStatus = new List<string>();
            List<Plan_UserSavedViews> NewListOfViews = new List<Plan_UserSavedViews>();
            List<Plan_UserSavedViews> listofsavedviews = objCommonFilter.LastSetOfViews(Sessions.User.ID, Sessions.PlanUserSavedViews, PresetName, isLoadPreset);
            if (isLoadPreset == true)
            {
                List<Preset> listofPreset = objCommonFilter.GetListofPreset(listofsavedviews, PresetName);

                return PartialView("~/Views/Shared/_DefaultViewFilters.cshtml", listofPreset);
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
                }
                List<string> SetOfStatus = listofsavedviews.Where(view => view.FilterName == StatusLabel).Select(View => View.FilterValues).ToList();
                if (SetOfStatus.Count > 0)
                {
                    if (SetOfStatus.FirstOrDefault() != null)
                    {
                        LastSetOfStatus = SetOfStatus.FirstOrDefault().Split(',').ToList();
                    }
                }
                string OwnerLabel = Convert.ToString(Enums.FilterLabel.Owner);

                List<string> LastSetOfOwners = new List<string>();

                string SetOfOwners = listofsavedviews.Where(view => view.FilterName == OwnerLabel).Select(View => View.FilterValues).FirstOrDefault();
                if (SetOfOwners != null)
                {
                    LastSetOfOwners = SetOfOwners.Split(',').ToList();
                }

                string TTLabel = Convert.ToString(Enums.FilterLabel.TacticType);
                List<string> LastSetOfTacticType = new List<string>();

                List<string> SetOfTacticType = listofsavedviews.Where(view => view.FilterName == TTLabel).Select(View => View.FilterValues).ToList();
                if (SetOfTacticType.Count > 0)
                {
                    if (SetOfTacticType.FirstOrDefault() != null)
                    {
                        LastSetOfTacticType = SetOfTacticType.FirstOrDefault().Split(',').ToList();
                    }
                }

                string YearLabel = Convert.ToString(Enums.FilterLabel.Year);
                List<string> LastSetOfYears = new List<string>();

                List<string> SetOfYears = listofsavedviews.Where(view => view.FilterName == YearLabel).Select(View => View.FilterValues).ToList();
                if (SetOfYears.Count > 0)
                {
                    if (SetOfYears.FirstOrDefault() != null)
                    {
                        LastSetOfYears = SetOfYears.FirstOrDefault().Split(',').ToList();
                    }
                }
                var LastSetofCustomField = listofsavedviews.Where(view => view.FilterName.Contains("CF")).Select(view => new { ID = view.FilterName, Value = view.FilterValues }).ToList();
                Sessions.FilterPresetName = null;
                return Json(new { StatusNAmes = LastSetOfStatus, Customfields = LastSetofCustomField, OwnerNames = LastSetOfOwners, TTList = LastSetOfTacticType, Years = LastSetOfYears }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get Owner List For Filter
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="PlanId">PlanId</param>
        /// <param name="ViewBy">ViewBy</param>
        /// <param name="ActiveMenu">ActiveMenu</param>
        /// <returns>returns Json as Owner List</returns>
        public JsonResult GetOwnerListForFilter(string PlanId, string ViewBy, string ActiveMenu)
        {
            try
            {
                var LoggedInUser = new OwnerModel
                {
                    OwnerId = Sessions.User.ID,
                    Title = Convert.ToString(Sessions.User.FirstName + " " + Sessions.User.LastName),
                };
                List<OwnerModel> lstOwner = objCommonFilter.GetOwnerListForFilter(Sessions.User.ID, Sessions.User.ID, Sessions.User.FirstName, Sessions.User.LastName, Sessions.ApplicationId, PlanId, ViewBy, ActiveMenu);
                return Json(new { isSuccess = true, AllowedOwner = lstOwner, LoggedInUser = LoggedInUser }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Nandish Shah.
        /// Action to save last accessed data
        /// </summary>
        /// <param name="PlanId">Plan Id and filters</param>
        public JsonResult SaveLastSetofViews(string planId, string customFieldIds, string ownerIds, string TacticTypeid, string StatusIds, string ViewName, string SelectedYears, string ParentCustomFieldsIds)
        {
            List<int> planIds = new List<int>();
            planIds = objCommonFilter.GetPlanIds(planId);

            #region "Remove previous records by userid"
            List<Plan_UserSavedViews> prevCustomFieldList = Sessions.PlanUserSavedViews;
            if (prevCustomFieldList != null && prevCustomFieldList.Count() > 0)
            {
                if (!string.IsNullOrEmpty(ViewName))
                {
                    bool IsViewNames = prevCustomFieldList.Where(custmfield => custmfield.ViewName == ViewName).Select(name => name.ViewName).Any();
                    if (IsViewNames)
                    {
                        return Json(new { isSuccess = false, msg = "Given Preset name already exists" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    prevCustomFieldList = prevCustomFieldList.Where(custmfield => custmfield.ViewName == null).ToList();
                }
            }            
            #endregion

            prevCustomFieldList = objCommonFilter.SaveLasSetofViews(planId, ViewName, ownerIds, TacticTypeid, StatusIds, SelectedYears, customFieldIds, ParentCustomFieldsIds, planIds, prevCustomFieldList, Sessions.User.ID);

            Sessions.PlanUserSavedViews = prevCustomFieldList;

            return Json(new { isSuccess = true, ViewName = ViewName }, JsonRequestBehavior.AllowGet);
        }
    }
}

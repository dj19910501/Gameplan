﻿using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Services;
using System;
using System.Collections.Generic;
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
        private MRPEntities objDbMrpEntities = new MRPEntities();
        #endregion

        /// <summary>
        /// Filter Index Page
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="activeMenu">activeMenu</param>
        /// <param name="currentPlanId">currentPlanId</param>
        /// <returns>returns partial view of Filter</returns>
        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Home, string CurrPlanIds = "")
        {
            int[] currentPlanId = CurrPlanIds.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            bool IsPlanEditable = false;
            bool isPublished = false;
            bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);

            //if (currentPlanId.Count() > 0)
            //{
            //    var lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            //    var objPlan = objDbMrpEntities.Plans.FirstOrDefault(_plan => (currentPlanId.Contains(_plan.PlanId)));

            //    if (objPlan.CreatedBy.Equals(Sessions.User.UserId))
            //    {
            //        IsPlanEditable = true;
            //    }
            //    else if (IsPlanEditAllAuthorized)
            //    {
            //        IsPlanEditable = true;
            //    }
            //    else if (IsPlanEditSubordinatesAuthorized)
            //    {
            //        if (lstOwnAndSubOrdinates.Contains(objPlan.CreatedBy))
            //        {
            //            IsPlanEditable = true;
            //        }
            //    }
            //    Plan Plan = objDbMrpEntities.Plans.FirstOrDefault(_plan => _plan.PlanId.Equals(currentPlanId));
            //    //isPublished = Plan.Status.Equals(Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString());
            //}
            ViewBag.ActiveMenu = activeMenu;
            ViewBag.IsPlanEditable = IsPlanEditable;
            ViewBag.IsPublished = isPublished;

            HomePlanModel planmodel = new Models.HomePlanModel();
            List<Plan> currentPlan = new List<Plan>();
            Plan latestPlan = new Plan();
            string currentYear = DateTime.Now.Year.ToString();
            List<int> modelIds = objDbMrpEntities.Models.Where(model => model.ClientId.Equals(Sessions.User.ClientId) && model.IsDeleted == false).Select(m => m.ModelId).ToList();
            List<Plan> activePlan = objDbMrpEntities.Plans.Where(p => modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true) && p.IsDeleted == false).ToList();
            string planPublishedStatus = Enums.PlanStatusValues.FirstOrDefault(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;

            if (activePlan.Count() > 0)
            {
                IsPlanEditable = true;
                ViewBag.IsPlanEditable = IsPlanEditable;
                latestPlan = activePlan.OrderBy(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).Select(plan => plan).FirstOrDefault();

                List<Plan> fiterActivePlan = new List<Plan>();
                fiterActivePlan = activePlan.Where(plan => Convert.ToInt32(plan.Year) < Convert.ToInt32(currentYear)).ToList();
                if (fiterActivePlan != null && fiterActivePlan.Any())
                {
                    latestPlan = fiterActivePlan.OrderByDescending(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).FirstOrDefault();
                }
                if (currentPlanId != null && currentPlanId.Count() != 0)
                {
                    currentPlan = activePlan.Where(p => currentPlanId.Contains(p.PlanId)).ToList();
                    if (currentPlan == null)
                    {
                        currentPlan[0] = latestPlan;
                        currentPlanId = currentPlan.Select(s => s.PlanId).ToArray();
                    }
                }
                else if (!Common.IsPlanPublished(Sessions.PlanId))
                {
                    if (Sessions.PublishedPlanId == 0)
                    {
                        fiterActivePlan = new List<Plan>();
                        fiterActivePlan = activePlan.Where(plan => plan.Year == currentYear && plan.Status.Equals(planPublishedStatus)).ToList();
                        if (fiterActivePlan != null && fiterActivePlan.Any())
                        {
                            currentPlan = fiterActivePlan.OrderBy(plan => plan.Title).ToList();
                        }
                        else
                        {
                            currentPlan[0] = latestPlan;
                        }
                    }
                    else
                    {
                        currentPlan = activePlan.Where(plan => plan.PlanId.Equals(Sessions.PublishedPlanId)).OrderBy(plan => plan.Title).ToList();
                        if (currentPlan == null)
                        {
                            currentPlan[0] = latestPlan;
                        }
                    }
                }
                else
                {
                    if (Sessions.PlanId == 0)
                    {
                        fiterActivePlan = new List<Plan>();
                        fiterActivePlan = activePlan.Where(plan => plan.Year == currentYear && plan.Status.Equals(planPublishedStatus)).ToList();
                        if (fiterActivePlan != null && fiterActivePlan.Any())
                        {
                            currentPlan = fiterActivePlan.OrderBy(plan => plan.Title).ToList();
                        }
                        else
                        {
                            currentPlan[0] = latestPlan;
                        }

                    }
                    else
                    {
                        currentPlan = activePlan.Where(plan => plan.PlanId.Equals(Sessions.PlanId)).ToList();
                        if (currentPlan == null)
                        {
                            currentPlan[0] = latestPlan;
                        }

                    }
                }
                //isPublished = currentPlan.Status.Equals(Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString());
                //ViewBag.IsPublished = isPublished;

                planmodel.PlanTitle = string.Join(",", currentPlan.Select(s => s.Title).ToArray());
                //planmodel.PlanId = currentPlan.PlanId;
                Sessions.PlanId = planmodel.PlanId;
                List<CustomFieldsForFilter> lstCustomField = new List<CustomFieldsForFilter>();
                List<CustomFieldsForFilter> lstCustomFieldOption = new List<CustomFieldsForFilter>();

                GetCustomFieldAndOptions(out lstCustomField, out lstCustomFieldOption);

                if (lstCustomField.Count > 0)
                {
                    if (lstCustomFieldOption.Count > 0)
                    {
                        planmodel.lstCustomFields = lstCustomField;
                        planmodel.lstCustomFieldOptions = lstCustomFieldOption;
                    }
                }
            }

            var Label = Enums.FilterLabel.Plan.ToString();
            var FilterName = Sessions.FilterPresetName;
            var SetOFLastViews = new List<Plan_UserSavedViews>();
            if (Sessions.PlanUserSavedViews == null)
            {
                SetOFLastViews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            }
            else
            {
                if (FilterName != null && FilterName != "")
                {
                    SetOFLastViews = Sessions.PlanUserSavedViews.ToList();
                }
                else
                {
                    SetOFLastViews = Sessions.PlanUserSavedViews.Where(view => view.ViewName == null).ToList();
                }
            }
            var SetOfPlanSelected = SetOFLastViews.Where(view => view.FilterName == Label && view.Userid == Sessions.User.UserId).ToList();
            Common.PlanUserSavedViews = SetOFLastViews;
            var LastSetOfPlanSelected = new List<string>();
            var LastSetOfYearSelected = new List<string>();
            var Yearlabel = Enums.FilterLabel.Year.ToString();

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
            //var SelectedYear = activePlan.Where(plan => plan.PlanId == currentPlan.PlanId).Select(plan => plan.Year).ToList();
            var SelectedYear = activePlan.Where(plan => currentPlan.Select(s => s.PlanId).ToArray().Contains(plan.PlanId)).Select(plan => plan.Year).ToList();

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

            var YearWiseListOfPlans = activePlan.Where(list => allPlanIds.Contains(list.PlanId)).ToList();

            planmodel.lstPlan = YearWiseListOfPlans.Select(plan => new PlanListModel
            {
                PlanId = plan.PlanId,
                Title = HttpUtility.HtmlDecode(plan.Title),
                Checked = LastSetOfPlanSelected.Count.Equals(0) ? currentPlan.Select(s => s.PlanId).ToArray().Contains(plan.PlanId) ? "checked" : "" : LastSetOfPlanSelected.Contains(plan.PlanId.ToString()) ? "checked" : "",
                Year = plan.Year

            }).Where(plan => !string.IsNullOrEmpty(plan.Title)).OrderBy(plan => plan.Title, new AlphaNumericComparer()).ToList();
            List<SelectListItem> lstYear = new List<SelectListItem>();
            var StartYears = CampPlans.Select(camp => camp.StartYear)
         .Distinct().ToList();

            var EndYears = CampPlans.Select(camp => camp.EndYear)
                .Distinct().ToList();

            var PlanYears = StartYears.Concat(EndYears).Distinct().ToList();

            var yearlist = PlanYears;
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

            ViewBag.ViewYear = lstYear.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();

            return PartialView("Filters", planmodel);
        }

        /// <summary>
        /// Get CustomField And Options
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param type="out" name="customFieldListOut">List of Custom Field</param>
        /// <param type="out" name="customFieldOptionsListOut">List of Custom Field Options</param>
        /// <returns></returns>
        public void GetCustomFieldAndOptions(out List<CustomFieldsForFilter> customFieldListOut, out List<CustomFieldsForFilter> customFieldOptionsListOut)
        {
            customFieldListOut = new List<CustomFieldsForFilter>();
            customFieldOptionsListOut = new List<CustomFieldsForFilter>();

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
                lstCustomFieldId = lstCustomField.Select(customField => customField.CustomFieldId).Distinct().ToList();

                var lstCustomFieldOptions = objDbMrpEntities.CustomFieldOptions
                                                            .Where(customFieldOption => lstCustomFieldId.Contains(customFieldOption.CustomFieldId) && customFieldOption.IsDeleted == false)
                                                            .Select(customFieldOption => new
                                                            {
                                                                customFieldOption.CustomFieldId,
                                                                customFieldOption.CustomFieldOptionId,
                                                                customFieldOption.Value
                                                            }).ToList();

                bool IsDefaultCustomRestrictionsViewable = Common.IsDefaultCustomRestrictionsViewable();

                var userCustomRestrictionList = Common.GetUserCustomRestrictionsList(Sessions.User.UserId, true);

                if (userCustomRestrictionList.Count() > 0)
                {
                    int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
                    int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                    int NonePermission = (int)Enums.CustomRestrictionPermission.None;

                    foreach (var customFieldId in lstCustomFieldId)
                    {
                        if (userCustomRestrictionList.Where(customRestriction => customRestriction.CustomFieldId == customFieldId).Count() > 0)
                        {
                            List<int> lstAllowedCustomFieldOptionIds = userCustomRestrictionList.Where(customRestriction => customRestriction.CustomFieldId == customFieldId &&
                                                                                                    (customRestriction.Permission == ViewOnlyPermission || customRestriction.Permission == ViewEditPermission))
                                                                                                .Select(customRestriction => customRestriction.CustomFieldOptionId).ToList();

                            List<int> lstRestrictedCustomFieldOptionIds = userCustomRestrictionList.Where(customRestriction => customRestriction.CustomFieldId == customFieldId &&
                                                                                                            customRestriction.Permission == NonePermission)
                                                                                                    .Select(customRestriction => customRestriction.CustomFieldOptionId).ToList();

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

                            if (IsDefaultCustomRestrictionsViewable)
                            {
                                var lstNewCustomFieldOptions = lstCustomFieldOptions.Where(option => !lstAllowedCustomFieldOptionIds.Contains(option.CustomFieldOptionId) && !lstRestrictedCustomFieldOptionIds.Contains(option.CustomFieldOptionId) && option.CustomFieldId == customFieldId).ToList();
                                if (lstNewCustomFieldOptions.Count() > 0)
                                {
                                    if (!(customFieldListOut.Where(customField => customField.CustomFieldId == customFieldId).Any()))
                                    {
                                        customFieldListOut.AddRange(lstCustomField.Where(customField => customField.CustomFieldId == customFieldId)
                                                                                    .Select(customField => new CustomFieldsForFilter()
                                                                                    {
                                                                                        CustomFieldId = customField.CustomFieldId,
                                                                                        Title = customField.Name
                                                                                    }).ToList());
                                    }

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
                customFieldListOut = customFieldListOut.OrderBy(customField => customField.Title).ToList();
            }
            if (customFieldOptionsListOut.Count() > 0)
            {
                customFieldOptionsListOut = customFieldOptionsListOut.OrderBy(customFieldOption => customFieldOption.CustomFieldId).ThenBy(customFieldOption => customFieldOption.Title).ToList();
            }
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
                List<TacticTypeModel> lstTacticType = objCommonFilter.GetTacticTypeListForFilter(PlanId, Sessions.User.UserId, Sessions.User.ClientId);
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
            var StatusLabel = Enums.FilterLabel.Status.ToString();
            var LastSetOfStatus = new List<string>();
            var NewListOfViews = new List<Plan_UserSavedViews>();
            List<Plan_UserSavedViews> listofsavedviews = objCommonFilter.LastSetOfViews(Sessions.User.UserId, Sessions.PlanUserSavedViews, PresetName, isLoadPreset);
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
                var SetOfStatus = listofsavedviews.Where(view => view.FilterName == StatusLabel).Select(View => View.FilterValues).ToList();
                if (SetOfStatus.Count > 0)
                {
                    if (SetOfStatus.FirstOrDefault() != null)
                    {
                        LastSetOfStatus = SetOfStatus.FirstOrDefault().Split(',').ToList();
                    }
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
                    OwnerId = Sessions.User.UserId.ToString(),
                    Title = Convert.ToString(Sessions.User.FirstName + " " + Sessions.User.LastName),
                };
                List<OwnerModel> lstOwner = objCommonFilter.GetOwnerListForFilter(Sessions.User.ClientId, Sessions.User.UserId, Sessions.User.FirstName, Sessions.User.LastName, Sessions.ApplicationId, PlanId, ViewBy, ActiveMenu);
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
    }
}
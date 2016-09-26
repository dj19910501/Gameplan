using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RevenuePlanner.Helpers;
using System.Threading.Tasks;
using System.Data;
using System.Web.Mvc;
using RevenuePlanner.Models;
using Elmah;
using RevenuePlanner.BDSService;
using System.Text;

namespace RevenuePlanner.Services
{
    public class Filter : IFilter
    {
        #region Variables
        StoredProcedure objSp = new StoredProcedure();
        CacheObject objCache = new CacheObject();
        private MRPEntities objDbMrpEntities = new MRPEntities();
        #endregion

        /// <summary>
        /// Get Fiter Data
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <returns>HomePlan Model</returns>
        public HomePlanModel GetFilterData(List<int> currentPlanId, int UserId, int ClientId, List<int> LstPlanId, string FilterPresetName, ref List<Plan_UserSavedViews> PlanUserSavedViews, ref List<SelectListItem> LstYear)
        {
            #region Declare Variables
            HomePlanModel planmodel = new Models.HomePlanModel();
            List<Plan> currentPlan = new List<Plan>();
            Plan latestPlan = new Plan();
            string currentYear = Convert.ToString(DateTime.Now.Year);
            #endregion

            //Get Active ModelIds for particular Client
            List<int> modelIds = objDbMrpEntities.Models.Where(model => model.ClientId.Equals(ClientId) && model.IsDeleted == false).Select(m => m.ModelId).ToList();
            //Get Active Plans for particular Model
            List<Plan> activePlan = objDbMrpEntities.Plans.Where(p => modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true) && p.IsDeleted == false).ToList();

            if (activePlan != null && activePlan.Count() > 0)
            {
                //Fetch Single plan from the list of Active Plan
                latestPlan = activePlan.OrderBy(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).Select(plan => plan).FirstOrDefault();

                List<Plan> fiterActivePlan = new List<Plan>();
                //Fetch List of Plans till previous year
                fiterActivePlan = activePlan.Where(plan => Convert.ToInt32(plan.Year) < Convert.ToInt32(currentYear)).ToList();
                if (fiterActivePlan != null && fiterActivePlan.Any())
                {
                    latestPlan = fiterActivePlan.OrderByDescending(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).FirstOrDefault();
                }
                
                // Set Current Plan
                SetCurrentPlan(ref currentPlanId, LstPlanId, ref currentPlan, latestPlan, currentYear, activePlan, ref fiterActivePlan);

                planmodel.PlanTitle = string.Join(",", currentPlan.Select(s => s.Title).ToArray());
                planmodel.lstPlanId = currentPlan.Select(s => s.PlanId).ToList();
                // Object for List of Custom Field
                List<CustomFieldsForFilter> lstCustomField = new List<CustomFieldsForFilter>();
                // Object for List of Custom Field Options
                List<CustomFieldsForFilter> lstCustomFieldOption = new List<CustomFieldsForFilter>();

                //Get Custom Field Options
                GetCustomFieldAndOptions(UserId, ClientId, out lstCustomField, out lstCustomFieldOption);

                if (lstCustomField.Count > 0)
                {
                    if (lstCustomFieldOption.Count > 0)
                    {
                        planmodel.lstCustomFields = lstCustomField;
                        planmodel.lstCustomFieldOptions = lstCustomFieldOption;
                    }
                }
            }

            string Label = Convert.ToString(Enums.FilterLabel.Plan);
            string FilterName = FilterPresetName;
            List<Plan_UserSavedViews> SetOFLastViews = new List<Plan_UserSavedViews>();

            // Set User's Last set of Views
            if (PlanUserSavedViews == null)
            {
                SetOFLastViews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == UserId).ToList();
            }
            else
            {
                if (FilterName != null && FilterName != "")
                {
                    SetOFLastViews = PlanUserSavedViews.ToList();
                }
                else
                {
                    SetOFLastViews = PlanUserSavedViews.Where(view => view.ViewName == null).ToList();
                }
            }

            // Feych List of Plan Saved Views wise
            List<Plan_UserSavedViews> SetOfPlanSelected = SetOFLastViews.Where(view => view.FilterName == Label && view.Userid == UserId).ToList();
            PlanUserSavedViews = SetOFLastViews;
            List<string> LastSetOfPlanSelected = new List<string>();
            List<string> LastSetOfYearSelected = new List<string>();
            string Yearlabel = Convert.ToString(Enums.FilterLabel.Year);

            List<Plan_UserSavedViews> SetofLastYearsSelected = SetOFLastViews.Where(view => view.FilterName == Yearlabel && view.Userid == UserId).ToList();
            string FinalSetOfPlanSelected = string.Empty;
            string FinalSetOfYearsSelected = string.Empty;
            if (!string.IsNullOrEmpty(FilterName))
            {
                //Get Plan as per Saved Filter wise
                FinalSetOfPlanSelected = SetOfPlanSelected.Where(view => view.ViewName == FilterName).Select(View => View.FilterValues).FirstOrDefault();
                //Get Year as per Saved Filter wise
                FinalSetOfYearsSelected = SetofLastYearsSelected.Where(view => view.ViewName == FilterName).Select(View => View.FilterValues).FirstOrDefault();
            }
            else
            {
                //Get Plan as per Default Preset wise
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

            activePlan = activePlan.Where(plan => plan.IsDeleted == false).ToList();
            List<string> SelectedYear = activePlan.Where(plan => currentPlan.Select(s => s.PlanId).ToArray().Contains(plan.PlanId)).Select(plan => plan.Year).ToList();

            if (LastSetOfYearSelected.Count > 0)
            {
                SelectedYear = activePlan.Where(plan => LastSetOfYearSelected.Contains(Convert.ToString(plan.Year))).Select(plan => plan.Year).Distinct().ToList();

                if (SelectedYear.Count == 0)
                {
                    SelectedYear = LastSetOfYearSelected;
                }
            }
            else
            {
                if (LastSetOfPlanSelected.Count > 0)
                {
                    SelectedYear = activePlan.Where(plan => LastSetOfPlanSelected.Contains(Convert.ToString(plan.PlanId))).Select(plan => plan.Year).Distinct().ToList();
                }
            }

            //List ofdistinct PlanIDs
            List<int> uniqueplanids = activePlan.Select(p => p.PlanId).Distinct().ToList();

            //List of Campaign based on Plan wise
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

            // List of Campaign between selected years
            List<int> CampPlanIds = CampPlans.Where(camp => SelectedYear.Contains(Convert.ToString(camp.StartDate.Year)) || SelectedYear.Contains(Convert.ToString(camp.EndDate.Year)))
                .Select(camp => camp.PlanId).Distinct().ToList();

            //List of planIds based on selected years
            List<int> PlanIds = activePlan.Where(plan => SelectedYear.Contains(plan.Year))
             .Select(plan => plan.PlanId).Distinct().ToList();

            List<int> allPlanIds = CampPlanIds.Concat(PlanIds).Distinct().ToList();

            List<Plan> YearWiseListOfPlans = activePlan.Where(list => allPlanIds.Contains(list.PlanId)).ToList();

            //All Plan List
            planmodel.lstPlan = YearWiseListOfPlans.Select(plan => new PlanListModel
            {
                PlanId = plan.PlanId,
                Title = HttpUtility.HtmlDecode(plan.Title),
                Checked = LastSetOfPlanSelected.Count.Equals(0) ? currentPlan.Select(s => s.PlanId).ToArray().Contains(plan.PlanId) ? "checked" : "" : LastSetOfPlanSelected.Contains(Convert.ToString(plan.PlanId)) ? "checked" : "",
                Year = plan.Year

            }).Where(plan => !string.IsNullOrEmpty(plan.Title)).OrderBy(plan => plan.Title, new AlphaNumericComparer()).ToList();

            List<SelectListItem> lstYear = new List<SelectListItem>();
            List<int> StartYears = CampPlans.Select(camp => camp.StartYear).Distinct().ToList();

            List<int> EndYears = CampPlans.Select(camp => camp.EndYear)
                .Distinct().ToList();

            List<int> PlanYears = StartYears.Concat(EndYears).Distinct().ToList();

            List<int> yearlist = PlanYears;
            SelectListItem objYear = new SelectListItem();
            foreach (int years in yearlist)
            {
                string yearname = Convert.ToString(years);
                objYear = new SelectListItem();

                objYear.Text = Convert.ToString(years);

                objYear.Value = yearname;
                objYear.Selected = SelectedYear.Contains(Convert.ToString(years)) ? true : false;
                lstYear.Add(objYear);
            }

            //List of Filter Year
            LstYear = lstYear.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();

            return planmodel;
        }

        /// <summary>
        /// Set Current Plan Details
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <returns></returns>
        private static void SetCurrentPlan(ref List<int> currentPlanId, List<int> LstPlanId, ref List<Plan> currentPlan, Plan latestPlan, string currentYear, List<Plan> activePlan, ref List<Plan> fiterActivePlan)
        {
            if (currentPlanId != null && currentPlanId.Count() != 0)
            {
                List<int> CurrPlan = currentPlanId;
                //Check whether current plan is in list of Active Plan or not
                currentPlan = activePlan.Where(p => CurrPlan.Contains(p.PlanId)).ToList();
                if (currentPlan == null)
                {
                    currentPlan[0] = latestPlan;
                    currentPlanId = currentPlan.Select(s => s.PlanId).ToList();
                }
            }
            else
            {
                //If Session of PlanIds null then fetch one plan from Active Plan List
                if (LstPlanId == null || LstPlanId.Count() == 0)
                {
                    fiterActivePlan = new List<Plan>();
                    fiterActivePlan = activePlan.Where(plan => plan.Year == currentYear).ToList();
                    if (fiterActivePlan != null && fiterActivePlan.Any())
                    {
                        currentPlan.Add(fiterActivePlan.OrderBy(plan => plan.Title).FirstOrDefault());
                    }
                    else
                    {
                        currentPlan[0] = latestPlan;
                    }

                }
                else
                {
                    //Check whether Session plan is in list of Active Plan or not
                    currentPlan = activePlan.Where(plan => LstPlanId.Contains(plan.PlanId)).ToList();
                    if (currentPlan == null)
                    {
                        currentPlan[0] = latestPlan;
                    }
                }
            }
        }

        /// <summary>
        /// Get CustomField And Options
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param type="out" name="customFieldListOut">List of Custom Field</param>
        /// <param type="out" name="customFieldOptionsListOut">List of Custom Field Options</param>
        /// <returns></returns>
        public void GetCustomFieldAndOptions(int UserId ,int ClientId, out List<CustomFieldsForFilter> customFieldListOut, out List<CustomFieldsForFilter> customFieldOptionsListOut)
        {
            customFieldListOut = new List<CustomFieldsForFilter>();
            customFieldOptionsListOut = new List<CustomFieldsForFilter>();

            string DropDownList = Convert.ToString(Enums.CustomFieldType.DropDownList);
            string EntityTypeTactic = Convert.ToString(Enums.EntityType.Tactic);
            var lstCustomField = objDbMrpEntities.CustomFields.Where(customField => customField.ClientId == ClientId && customField.IsDeleted.Equals(false) &&
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

                List<RevenuePlanner.Models.CustomRestriction> userCustomRestrictionList = Common.GetUserCustomRestrictionsList(UserId, true);

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
        /// <param name="UserId">UserId</param>
        /// <param name="ClientId">ClientId</param>
        /// <returns>returns List of TacticTypeModel</returns>
        public List<TacticTypeModel> GetTacticTypeListForFilter(string PlanId, int UserId, int ClientId)
        {
            try
            {
                DataSet dsPlanCampProgTac = new DataSet();
                dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(PlanId);
                objCache.AddCache(Convert.ToString(Enums.CacheObject.dsPlanCampProgTac), dsPlanCampProgTac);

                List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                objCache.AddCache(Convert.ToString(Enums.CacheObject.Plan), lstPlans);

                List<Plan_Campaign> lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                objCache.AddCache(Convert.ToString(Enums.CacheObject.Campaign), lstCampaign);

                List<Custom_Plan_Campaign_Program> lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                objCache.AddCache(Convert.ToString(Enums.CacheObject.Program), lstProgramPer);

                List<Custom_Plan_Campaign_Program_Tactic> customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                objCache.AddCache(Convert.ToString(Enums.CacheObject.CustomTactic), customtacticList);
                objCache.AddCache(Convert.ToString(Enums.CacheObject.PlanTacticListforpackageing), customtacticList);
                List<Plan_Campaign_Program_Tactic> tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                objCache.AddCache(Convert.ToString(Enums.CacheObject.Tactic), tacticList);

                List<int> planTacticIds = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
                List<int> lstAllowedEntityIds = Common.GetViewableTacticList(UserId, ClientId, planTacticIds, false);
                return GetTacticTypeList(tacticList, lstAllowedEntityIds, UserId);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }

            return null;
        }

        /// <summary>
        /// Get TacticType List
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="tacticList">List of Tactic</param>
        /// <param name="lstAllowedEntityIds">List of Allowed EntityId</param>
        /// <param name="UserId">UserId</param>
        /// <returns>returns List of TacticTypeModel</returns>
        public List<TacticTypeModel> GetTacticTypeList(List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstAllowedEntityIds, int UserId)
        {
            List<Plan_Campaign_Program_Tactic> TacticUserList = tacticList.ToList();
            if (TacticUserList.Count > 0)
            {
                TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == UserId).ToList();
                lstAllowedEntityIds = TacticUserList.Select(a => a.PlanTacticId).ToList();
            }
            List<TacticTypeModel> objTacticType = objSp.GetTacticTypeList(string.Join(",", lstAllowedEntityIds));
            return objTacticType;
        }

        /// <summary>
        /// Get Last Set Of Views
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="UserId">UserId</param>
        /// <param name="PlanUserSavedViews">List of Saved Plan-User View</param>
        /// <param name="PresetName">Preset Name</param>
        /// <param name="isLoadPreset">isLoadPreset</param>
        /// <returns>returns List of Last Set Of Views</returns>
        public List<Plan_UserSavedViews> LastSetOfViews(int UserId, List<Plan_UserSavedViews> PlanUserSavedViews, string PresetName = "", Boolean isLoadPreset = false)
        {
            string StatusLabel = Convert.ToString(Enums.FilterLabel.Status);
            List<string> LastSetOfStatus = new List<string>();
            List<Plan_UserSavedViews> ListofSavedViews = new List<Plan_UserSavedViews>();
            if (PlanUserSavedViews == null || isLoadPreset == true)
            {
                ListofSavedViews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == UserId).Select(view => view).ToList();
            }
            else
            {
                ListofSavedViews = PlanUserSavedViews;
            }
            return ListofSavedViews;
        }

        /// <summary>
        /// Get List of Preset
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="listofsavedviews">List of saved views</param>
        /// <param name="PresetName">Preset Name</param>
        /// <returns>returns List of LastSetOfViews</returns>
        public List<Preset> GetListofPreset(List<Plan_UserSavedViews> listofsavedviews, string PresetName = "")
        {
            List<Preset> newList = new List<Preset>();
            newList = (from item in listofsavedviews
                       where item.ViewName != null
                       select new Preset
                       {
                           Id = Convert.ToString(item.Id),
                           Name = item.ViewName,
                           IsDefaultPreset = item.IsDefaultPreset
                       }).ToList();

            newList = newList.GroupBy(g => g.Name).Select(x => x.FirstOrDefault()).OrderBy(g => g.Name).ToList();
            if (!string.IsNullOrEmpty(PresetName))
            {
                newList = new List<Preset>();
                newList = (from item in listofsavedviews
                           where item.ViewName != null && item.ViewName.ToUpper().Contains(PresetName.ToUpper())
                           select new Preset
                           {
                               Id = Convert.ToString(item.Id),
                               Name = item.ViewName,
                               IsDefaultPreset = item.IsDefaultPreset
                           }).ToList();
                newList = newList.GroupBy(g => g.Name).Select(x => x.FirstOrDefault()).OrderBy(g => g.Name).ToList();
            }

            return newList;
        }

        /// <summary>
        /// Get Owner List For Filter
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="ClientId">ClientId</param>
        /// <param name="UserId">UserId</param>
        /// <param name="FirstName">User's First Name</param>
        /// <param name="LastName">User's Last Name</param>
        /// <param name="ApplicationId">ApplicationId</param>
        /// <param name="PlanId">PlanId</param>
        /// <param name="ViewBy">ViewBy</param>
        /// <param name="ActiveMenu">ActiveMenu</param>
        /// <returns>returns List of Owner</returns>
        public List<OwnerModel> GetOwnerListForFilter(int ClientId, int UserId, string FirstName, string LastName, Guid ApplicationId, string PlanId, string ViewBy, string ActiveMenu)
        {
            try
            {
                List<int> lstAllowedEntityIds = new List<int>();
                List<int> PlanIds = string.IsNullOrWhiteSpace(PlanId) ? new List<int>() : PlanId.Split(',').Select(plan => int.Parse(plan)).ToList();
                DataSet dsPlanCampProgTac = new DataSet();
                dsPlanCampProgTac = (DataSet)objCache.Returncache(Convert.ToString(Enums.CacheObject.dsPlanCampProgTac));
                if (dsPlanCampProgTac == null)
                {
                    dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(PlanId);
                }
                List<Plan_Campaign> campaignList = new List<Plan_Campaign>();
                if (dsPlanCampProgTac != null && dsPlanCampProgTac.Tables[1] != null)
                {
                    campaignList = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]);
                }

                objCache.AddCache(Convert.ToString(Enums.CacheObject.Campaign), campaignList);
                List<int> campaignListids = campaignList.Select(campaign => campaign.PlanCampaignId).ToList();
                List<Plan_Campaign_Program> programList = new List<Plan_Campaign_Program>();
                if (dsPlanCampProgTac != null && dsPlanCampProgTac.Tables[2] != null)
                {
                    programList = Common.GetSpProgramList(dsPlanCampProgTac.Tables[2]);
                }
                objCache.AddCache(Convert.ToString(Enums.CacheObject.Program), programList);

                List<int> programListids = programList.Select(program => program.PlanProgramId).ToList();
                List<Custom_Plan_Campaign_Program_Tactic> customtacticList = (List<Custom_Plan_Campaign_Program_Tactic>)objCache.Returncache(Convert.ToString(Enums.CacheObject.CustomTactic));
                if (customtacticList == null || customtacticList.Count == 0)
                {
                    customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                }
                objCache.AddCache(Convert.ToString(Enums.CacheObject.CustomTactic), customtacticList);
                List<Plan_Campaign_Program_Tactic> tacticList = (List<Plan_Campaign_Program_Tactic>)objCache.Returncache(Convert.ToString(Enums.CacheObject.Tactic));
                if (tacticList == null || tacticList.Count == 0)
                {
                    tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                }
                objCache.AddCache(Convert.ToString(Enums.CacheObject.Tactic), tacticList);
                string section = Convert.ToString(Enums.Section.Tactic);

                List<CustomField> customfield = objDbMrpEntities.CustomFields.Where(customField => customField.EntityType == section && customField.ClientId == ClientId && customField.IsDeleted == false).ToList();
                objCache.AddCache(Convert.ToString(Enums.CacheObject.CustomField), customfield);
                List<int> customfieldidlist = customfield.Select(c => c.CustomFieldId).ToList();
                var lstAllTacticCustomFieldEntitiesanony = objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => customfieldidlist.Contains(customFieldEntity.CustomFieldId))
                                                                                       .Select(customFieldEntity => new CacheCustomField { EntityId = customFieldEntity.EntityId, CustomFieldId = customFieldEntity.CustomFieldId, Value = customFieldEntity.Value, CreatedBy = customFieldEntity.CreatedBy, CustomFieldEntityId = customFieldEntity.CustomFieldEntityId }).Distinct().ToList();

                objCache.AddCache(Convert.ToString(Enums.CacheObject.CustomFieldEntity), lstAllTacticCustomFieldEntitiesanony);
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

                    List<int> AllowedEntityIds = Common.GetViewableTacticList(UserId, ClientId, planTacticIds, false, customfieldlist);
                    if (AllowedEntityIds.Count > 0)
                    {
                        lstAllowedEntityIds.AddRange(AllowedEntityIds);
                    }

                }

                return GetOwnerList(ViewBy, ActiveMenu, tacticList, lstAllowedEntityIds, ApplicationId, UserId);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }

            return null;
        }

        /// <summary>
        /// Get Owner List
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="ViewBy">ViewBy</param>
        /// <param name="ActiveMenu">ActiveMenu</param>
        /// <param name="tacticList">List of tactic</param>
        /// <param name="lstAllowedEntityIds">List of Allowed EntityIds</param>
        /// <param name="ApplicationId">ApplicationId</param>
        /// <param name="UserId">UserId</param>
        /// <returns>returns List of Owner</returns>
        public List<OwnerModel> GetOwnerList(string ViewBy, string ActiveMenu, List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstAllowedEntityIds, Guid ApplicationId, int UserId)
        {
            List<User> lstOwners = GetIndividualsByPlanId(ViewBy, ActiveMenu, tacticList, lstAllowedEntityIds, ApplicationId, UserId);
            List<OwnerModel> lstAllowedOwners = lstOwners.Select(owner => new OwnerModel
            {
                OwnerId = owner.ID,
                Title = owner.FirstName + " " + owner.LastName,
            }).Distinct().OrderBy(owner => owner.Title).ToList();

            lstAllowedOwners = lstAllowedOwners.Where(owner => !string.IsNullOrEmpty(owner.Title)).OrderBy(owner => owner.Title, new AlphaNumericComparer()).ToList();
            return lstAllowedOwners;
        }

        /// <summary>
        /// Get Individuals By PlanId
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="ViewBy">ViewBy</param>
        /// <param name="ActiveMenu">ActiveMenu</param>
        /// <param name="tacticList">List of tactic</param>
        /// <param name="lstAllowedEntityIds">List of Allowed EntityIds</param>
        /// <param name="ApplicationId">ApplicationId</param>
        /// <param name="UserId">UserId</param>
        /// <returns>returns List of Individuals By PlanId</returns>
        public List<User> GetIndividualsByPlanId(string ViewBy, string ActiveMenu, List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstAllowedEntityIds, Guid ApplicationId, int UserId)
        {
            BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();
            List<Plan_Campaign_Program_Tactic> TacticUserList = tacticList.Distinct().ToList();
            if (TacticUserList.Count > 0)
            {
                TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == UserId).ToList();
            }
            List<int> useridslist = TacticUserList.Select(tactic => tactic.CreatedBy).Distinct().ToList();
            List<User> individuals = bdsUserRepository.GetMultipleTeamMemberNameByApplicationIdEx(useridslist, ApplicationId);
            return individuals;
        }

        /// <summary>
        /// Get List of PlanId
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="UserId">planId</param>
        /// <returns>returns List of PlanId</returns>
        public List<int> GetPlanIds(string planId)
        {
            List<int> planIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(planId))
            {
                planIds = planId.Split(',').Select(plan => int.Parse(plan)).ToList();
            }
            List<Plan> ListofPlans = objDbMrpEntities.Plans.Where(p => planIds.Contains(p.PlanId)).ToList();
            planIds = ListofPlans.Select(plan => plan.PlanId).ToList();
            return planIds;
        }

        /// <summary>
        /// Save Las Set of Views
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="planId">planId</param>
        /// <param name="ViewName">ViewName</param>
        /// <param name="ownerIds">ownerIds</param>
        /// <param name="TacticTypeid">TacticTypeid</param>
        /// <param name="StatusIds">StatusIds</param>
        /// <param name="SelectedYears">SelectedYears</param>
        /// <param name="customFieldIds">customFieldIds</param>
        /// <param name="ParentCustomFieldsIds">ParentCustomFieldsIds</param>
        /// <param name="planIds">planIds</param>
        /// <param name="prevCustomFieldList">prevCustomFieldList</param>
        /// <param name="UserId">UserId</param>
        /// <returns>returns List of PlanId</returns>
        public List<Plan_UserSavedViews> SaveLasSetofViews(string planId, string ViewName, string ownerIds, string TacticTypeid, string StatusIds, string SelectedYears, string customFieldIds, string ParentCustomFieldsIds, List<int> planIds, List<Plan_UserSavedViews> prevCustomFieldList, int UserId)
        {
            List<Plan_UserSavedViews> NewCustomFieldData = new List<Plan_UserSavedViews>();
            #region "Save filter values to Plan_UserSavedViews"
            NewCustomFieldData = SaveFilterValues(planId, ViewName, ownerIds, TacticTypeid, StatusIds, SelectedYears, planIds, UserId);

            string[] filterValues = { };
            string[] filteredCustomFields = { }; //Array of Combination Custom Field Ids and Custom Field Option Ids
            string PrefixCustom = "CF_"; // String for prefix of Custom Field Id
            StringBuilder FilterName = new StringBuilder(); // String builder to store Existing Filter Values
            string Previousval = string.Empty; // Custom Filed Value without Prefix
            string PreviousValue = string.Empty; // Custom Filed Value with Prefix
            string CustomOptionvalue = string.Empty; // Custom Option Value

            Plan_UserSavedViews ExistingFieldlist = new Plan_UserSavedViews();
            if (!string.IsNullOrEmpty(customFieldIds))
            {
                if (string.IsNullOrWhiteSpace(customFieldIds))
                {
                    filteredCustomFields = null;
                }
                else
                {
                    filteredCustomFields = customFieldIds.Split(',');
                }
                if (filteredCustomFields != null)
                {
                    SaveCustomFilterValues(ViewName, NewCustomFieldData, ref filterValues, filteredCustomFields, PrefixCustom, FilterName, ref Previousval, ref PreviousValue, ref CustomOptionvalue, ref ExistingFieldlist, UserId, prevCustomFieldList);
                }
            }
            else
            {
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
                        string FilterNameWithPrefix = PrefixCustom + customField;
                        NewCustomFieldData.Add(InsertLastViewedUserData("", ViewName, FilterNameWithPrefix, UserId));
                    }
                }
            }
            if (StatusIds == "Report")
            {
                List<Plan_UserSavedViews> statulist = prevCustomFieldList.Where(a => a.FilterName == Convert.ToString(Enums.FilterLabel.Status)).ToList();
                prevCustomFieldList = prevCustomFieldList.Except(statulist).ToList();
            }
            if (ViewName != null)
            {
                objDbMrpEntities.SaveChanges();
            }
            else
            {
                bool isCheckinPrev = false;
                bool isCheckinNew = false;
                if (prevCustomFieldList != null)
                {
                    isCheckinPrev = prevCustomFieldList.Select(a => a.FilterValues).Except(NewCustomFieldData.Select(b => b.FilterValues)).Any();
                    if (NewCustomFieldData != null)
                    {
                        isCheckinNew = NewCustomFieldData.Select(a => a.FilterValues).Except(prevCustomFieldList.Select(b => b.FilterValues)).Any();
                    }
                }
                
                if (isCheckinPrev || isCheckinNew)
                {
                    List<int> ids = prevCustomFieldList.Select(t => t.Id).ToList();
                    string ListOfPreviousIDs = null;
                    if (ids.Count > 0)
                    {
                        ListOfPreviousIDs = string.Join(",", ids);
                    }

                    objDbMrpEntities.DeleteLastViewedData(UserId, ListOfPreviousIDs); //Sp to delete last viewed data before inserting new one.
                    objDbMrpEntities.SaveChanges();
                }
                else
                {
                    if (prevCustomFieldList != null && prevCustomFieldList.Count == 0)
                    {
                        objDbMrpEntities.SaveChanges();
                    }
                }
            }
            return objDbMrpEntities.Plan_UserSavedViews.Where(custmfield => custmfield.Userid == UserId).ToList();            
            #endregion
        }

        /// <summary>
        /// Save Custom Filter Values
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="ViewName">ViewName</param>
        /// <param name="NewCustomFieldData">NewCustomFieldData</param>
        /// <param name="filterValues">filterValues</param>
        /// <param name="filteredCustomFields">filteredCustomFields</param>
        /// <param name="PrefixCustom">PrefixCustom</param>
        /// <param name="FilterName">FilterName</param>
        /// <param name="Previousval">Previousval</param>
        /// <param name="PreviousValue">PreviousValue</param>
        /// <param name="CustomOptionvalue">CustomOptionvalue</param>
        /// <param name="ExistingFieldlist">ExistingFieldlist</param>
        /// <param name="UserId">UserId</param>
        /// <returns>Save Custom Filter Values</returns>
        private void SaveCustomFilterValues(string ViewName, List<Plan_UserSavedViews> NewCustomFieldData, ref string[] filterValues, string[] filteredCustomFields, string PrefixCustom, StringBuilder FilterName, ref string Previousval, ref string PreviousValue, ref string CustomOptionvalue, ref Plan_UserSavedViews ExistingFieldlist, int UserId, List<Plan_UserSavedViews> StoredUserView)
        {
            Plan_UserSavedViews objFilterValues = new Plan_UserSavedViews();
            List<Plan_UserSavedViews> listLineitem = StoredUserView;
            foreach (string customField in filteredCustomFields)
            {
                filterValues = customField.Split('_');
                if (filterValues.Count() > 1)
                {
                    CustomOptionvalue = filterValues[1];
                }
                bool IsFilterAddinDb = false;
                if (filterValues != null && filterValues.Count() > 0)
                {
                    if (filterValues[0] != null)
                    {
                        PreviousValue = PrefixCustom + Convert.ToString(filterValues[0]);
                    }
                    
                    string PrevVal = PreviousValue;
                    if (listLineitem != null)
                    {
                        ExistingFieldlist = listLineitem.Where(w => w.FilterName.Equals(PrevVal)).FirstOrDefault();
                    }
                    
                    if (FilterName == null)
                    {
                        if (ExistingFieldlist != null)
                        {
                            FilterName.Append(ExistingFieldlist.FilterValues);
                        }
                        else
                        {
                            FilterName.Append("");
                        }
                    }
                    else
                    {
                        FilterName.Append("");
                    }
                    if (FilterName != null && Convert.ToString(FilterName) == PreviousValue && !string.IsNullOrEmpty(CustomOptionvalue))
                    {
                        Previousval += ',' + CustomOptionvalue;
                        objFilterValues.FilterValues = Previousval;
                    }
                    else
                    {
                        string FilterNameWithPrefix = PrefixCustom + filterValues[0];
                        FilterName.Length = 0;
                        FilterName.Append(PrefixCustom + Convert.ToString(filterValues[0]));
                        Previousval = "";
                        Previousval = CustomOptionvalue;
                        NewCustomFieldData.Add(InsertLastViewedUserData(CustomOptionvalue, ViewName, FilterNameWithPrefix, UserId));
                        IsFilterAddinDb = true;
                    }

                }
                if (!IsFilterAddinDb)
                {
                    NewCustomFieldData.Add(objFilterValues);
                }                
            }
        }

        /// <summary>
        /// Save Filter Values
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <param name="planId">planId</param>
        /// <param name="ViewName">ViewName</param>
        /// <param name="ownerIds">ownerIds</param>
        /// <param name="TacticTypeid">TacticTypeid</param>
        /// <param name="StatusIds">StatusIds</param>
        /// <param name="SelectedYears">SelectedYears</param>
        /// <param name="planIds">planIds</param>
        /// <param name="UserId">UserId</param>
        /// <returns>Returns List of Saved Views</returns>
        private List<Plan_UserSavedViews> SaveFilterValues(string planId, string ViewName, string ownerIds, string TacticTypeid, string StatusIds, string SelectedYears, List<int> planIds, int UserId)
        {
            List<Plan_UserSavedViews> NewCustomFieldData = new List<Plan_UserSavedViews>();
            if (planIds.Count != 0)
            {
                NewCustomFieldData.Add(InsertLastViewedUserData(planId, ViewName, Convert.ToString(Enums.FilterLabel.Plan), UserId));
            }
            NewCustomFieldData.Add(InsertLastViewedUserData(ownerIds, ViewName, Convert.ToString(Enums.FilterLabel.Owner), UserId));
            NewCustomFieldData.Add(InsertLastViewedUserData(TacticTypeid, ViewName, Convert.ToString(Enums.FilterLabel.TacticType), UserId));
            if (StatusIds != "Report")
            {
                NewCustomFieldData.Add(InsertLastViewedUserData(StatusIds, ViewName, Convert.ToString(Enums.FilterLabel.Status), UserId));
            }
            if (SelectedYears != null && SelectedYears != "")
            {
                NewCustomFieldData.Add(InsertLastViewedUserData(SelectedYears, ViewName, Convert.ToString(Enums.FilterLabel.Year), UserId));
            }
            return NewCustomFieldData;
        }

        /// <summary>
        /// Added By: Nandish Shah.
        /// Desc : Function to insert data into Plan_UserSaved Views table for filters in left pane.
        /// </summary>
        /// <param name="Ids">Ids</param>
        /// <param name="ViewName">ViewName</param>
        /// <param name="FilterName">FilterName</param>
        /// <param name="UserId">UserId</param>
        /// <param name="NewCustomFieldData">NewCustomFieldData</param>
        private Plan_UserSavedViews InsertLastViewedUserData(string Ids, string ViewName, string FilterName, int UserId)
        {
            Plan_UserSavedViews objFilterValues = new Plan_UserSavedViews();
            objFilterValues.ViewName = null;
            if (ViewName != null && ViewName != "")
            {
                objFilterValues.ViewName = ViewName;
            }
            objFilterValues.FilterName = FilterName;
            objFilterValues.FilterValues = Ids;
            objFilterValues.Userid = UserId;
            objFilterValues.LastModifiedDate = DateTime.Now;
            objFilterValues.IsDefaultPreset = false;
            objDbMrpEntities.Entry(objFilterValues).State = EntityState.Added;
            return objFilterValues;
        }
    }
}
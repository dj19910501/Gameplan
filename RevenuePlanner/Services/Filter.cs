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
        private StoredProcedure objSp;
        private CacheObject objCache;
        private MRPEntities objDbMrpEntities;
        private BDSService.BDSServiceClient bdsUserRepository;
        public Filter()
        {
            objSp = new StoredProcedure();
            objCache = new CacheObject();
            objDbMrpEntities = new MRPEntities();
            bdsUserRepository = new BDSService.BDSServiceClient();
        }
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

            //Get Active Plans for particular Model of a client 
            List<Plan> activePlan = (from plan in objDbMrpEntities.Plans
                                     join model in objDbMrpEntities.Models on plan.ModelId equals model.ModelId
                                     where model.ClientId.Equals(ClientId) && plan.IsDeleted == false && plan.IsActive == true && model.IsDeleted == false
                                     select plan
                                    ).ToList();


            if (activePlan != null && activePlan.Count() > 0)
            {
                //Set Plan Details
                SetPlanDetails(currentPlanId, UserId, ClientId, LstPlanId, planmodel, ref currentPlan, latestPlan, currentYear, activePlan);
            }

            //Set Last User Saved Views
            List<Plan_UserSavedViews> SetOFLastViews = SetLastViews(UserId, PlanUserSavedViews, FilterPresetName);

            // Fetch List of Plan Saved Views wise
            List<Plan_UserSavedViews> SetOfPlanSelected = SetOFLastViews.Where(view => view.FilterName == Convert.ToString(Enums.FilterLabel.Plan) && view.Userid == UserId).ToList();
            PlanUserSavedViews = SetOFLastViews;
            List<string> LastSetOfPlanSelected = new List<string>();
            List<string> LastSetOfYearSelected = new List<string>();
            List<Plan> YearWiseListOfPlans = new List<Plan>();
            List<int> StartYears = new List<int>();
            List<int> EndYears = new List<int>();
            List<string> SelectedYear = activePlan.Where(plan => plan.IsDeleted == false && currentPlan.Select(s => s.PlanId).ToArray().Contains(plan.PlanId)).Select(plan => plan.Year).ToList();
            //List ofdistinct PlanIDs
            List<int> uniqueplanids = activePlan.Select(p => p.PlanId).Distinct().ToList();
            //List of Campaign based on Plan wise
            List<Plan_Campaign> CampPlans = objDbMrpEntities.Plan_Campaign.Where(camp => camp.IsDeleted == false && uniqueplanids.Contains(camp.PlanId)).Select(camp => camp).ToList();
            if (currentPlanId == null || currentPlanId.Count() == 0)
            {
                //Set Plan And Year as per Last set of views
                SetLastSetofPlanYear(UserId, FilterPresetName, SetOFLastViews, SetOfPlanSelected, ref LastSetOfPlanSelected, ref LastSetOfYearSelected, Convert.ToString(Enums.FilterLabel.Year));

                //Fetch Selected Year

                SelectedYear = GetSelectedYear(activePlan, LastSetOfPlanSelected, LastSetOfYearSelected, SelectedYear);
            }
            // List of Campaign between selected years
            List<int> CampPlanIds = CampPlans.Where(camp => SelectedYear.Contains(Convert.ToString(camp.StartDate.Year)) || SelectedYear.Contains(Convert.ToString(camp.EndDate.Year)))
                .Select(camp => camp.PlanId).Distinct().ToList();

            //List of planIds based on selected years
            List<int> PlanIds = activePlan.Where(plan => SelectedYear.Contains(plan.Year))
             .Select(plan => plan.PlanId).Distinct().ToList();

            List<int> allPlanIds = CampPlanIds.Concat(PlanIds).Distinct().ToList();

            YearWiseListOfPlans = activePlan.Where(list => allPlanIds.Contains(list.PlanId)).ToList();

            //All Plan List
            planmodel.lstPlan = YearWiseListOfPlans.Select(plan => new PlanListModel
            {
                PlanId = plan.PlanId,
                Title = HttpUtility.HtmlDecode(plan.Title),
                Checked = LastSetOfPlanSelected.Count.Equals(0) ? currentPlan.Select(s => s.PlanId).ToArray().Contains(plan.PlanId) ? "checked" : "" : LastSetOfPlanSelected.Contains(Convert.ToString(plan.PlanId)) ? "checked" : "",
                Year = plan.Year

            }).Where(plan => !string.IsNullOrEmpty(plan.Title)).OrderBy(plan => plan.Title, new AlphaNumericComparer()).ToList();

            if (CampPlans != null && CampPlans.Count() > 0)
            {
                StartYears = CampPlans.Select(camp => camp.StartDate.Year).Distinct().ToList();
                EndYears = CampPlans.Select(camp => camp.EndDate.Year).Distinct().ToList();
            }
            else
            {
                StartYears = activePlan.Select(camp => Convert.ToInt32(camp.Year)).Distinct().ToList();
            }

            LstYear = GetListofYears(SelectedYear, StartYears, EndYears);

            return planmodel;
        }

        /// <summary>
        /// Get List of Years
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        public List<SelectListItem> GetListofYears(List<string> SelectedYear, List<int> StartYears, List<int> EndYears)
        {
            List<int> PlanYears = StartYears.Concat(EndYears).Distinct().ToList();
            List<SelectListItem> LstYear = new List<SelectListItem>();

            List<int> yearlist = PlanYears;
            SelectListItem objYear = new SelectListItem();
            foreach (int years in yearlist)
            {
                string yearname = Convert.ToString(years);
                objYear = new SelectListItem();

                objYear.Text = Convert.ToString(years);

                objYear.Value = yearname;
                objYear.Selected = SelectedYear.Contains(Convert.ToString(years)) ? true : false;
                LstYear.Add(objYear);
            }

            //List of Filter Year
            LstYear = LstYear.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            return LstYear;
        }

        /// <summary>
        /// Get Selected Year
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        public List<string> GetSelectedYear(List<Plan> activePlan, List<string> LastSetOfPlanSelected, List<string> LastSetOfYearSelected, List<string> SelectedYear)
        {
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
            return SelectedYear;
        }

        /// <summary>
        /// Set Last Set of Plan Year
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        public void SetLastSetofPlanYear(int UserId, string FilterName, List<Plan_UserSavedViews> SetOFLastViews, List<Plan_UserSavedViews> SetOfPlanSelected, ref List<string> LastSetOfPlanSelected, ref List<string> LastSetOfYearSelected, string Yearlabel)
        {
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
        }

        /// <summary>
        /// Set Last Views
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        public List<Plan_UserSavedViews> SetLastViews(int UserId, List<Plan_UserSavedViews> PlanUserSavedViews, string FilterName)
        {
            List<Plan_UserSavedViews> SetOFLastViews;
            // Set User's Last set of Views
            if (PlanUserSavedViews == null)
            {
                SetOFLastViews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == UserId).ToList();
            }
            else
            {
                if (!string.IsNullOrEmpty(FilterName))
                {
                    SetOFLastViews = PlanUserSavedViews.Where(view => view.ViewName == FilterName).ToList();
                }
                else
                {
                    SetOFLastViews = PlanUserSavedViews.Where(view => view.ViewName == null).ToList();
                }
            }
            return SetOFLastViews;
        }

        /// <summary>
        /// Set Plan Details
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        public void SetPlanDetails(List<int> currentPlanId, int UserId, int ClientId, List<int> LstPlanId, HomePlanModel planmodel, ref List<Plan> currentPlan, Plan latestPlan, string currentYear, List<Plan> activePlan)
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
            SetCurrentPlan(currentPlanId, LstPlanId, ref currentPlan, latestPlan, currentYear, activePlan);

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
            else
            {
                planmodel.lstCustomFields = new List<CustomFieldsForFilter>();
                planmodel.lstCustomFieldOptions = new List<CustomFieldsForFilter>();
            }
        }

        /// <summary>
        /// Set Current Plan Details
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <returns></returns>
        public void SetCurrentPlan(List<int> SelectedPlanId, List<int> StoredPlanId, ref List<Plan> LstPlan, Plan DefaultPlan, string CurrentYear, List<Plan> ActivePlan)
        {
            if (SelectedPlanId != null && SelectedPlanId.Count() != 0)
            {
                //Check whether current plan is in list of Active Plan or not
                LstPlan = ActivePlan.Where(p => SelectedPlanId.Contains(p.PlanId)).ToList();
                if (LstPlan == null || LstPlan.Count == 0)
                {
                    LstPlan.Add(DefaultPlan);
                    SelectedPlanId = LstPlan.Select(s => s.PlanId).ToList();
                }
            }
            else
            {
                //If Session of PlanIds null then fetch one plan from Active Plan List
                if (StoredPlanId == null || StoredPlanId.Count() == 0)
                {
                    List<Plan> fiterActivePlan;
                    fiterActivePlan = ActivePlan.Where(plan => plan.Year == CurrentYear).ToList();
                    if (fiterActivePlan != null && fiterActivePlan.Any())
                    {
                        LstPlan.Add(fiterActivePlan.OrderBy(plan => plan.Title).FirstOrDefault());
                    }
                    else
                    {
                        LstPlan.Add(DefaultPlan);
                    }

                }
                else
                {
                    //Check whether Session plan is in list of Active Plan or not
                    LstPlan = ActivePlan.Where(plan => StoredPlanId.Contains(plan.PlanId)).ToList();
                    if (LstPlan == null || LstPlan.Count == 0)
                    {
                        LstPlan.Add(DefaultPlan);
                    }
                }
            }
        }

        /// <summary>
        /// Get CustomField And Options
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        public void GetCustomFieldAndOptions(int UserId, int ClientId, out List<CustomFieldsForFilter> customFieldListOut, out List<CustomFieldsForFilter> customFieldOptionsListOut)
        {
            #region Variables
            customFieldListOut = new List<CustomFieldsForFilter>();
            customFieldOptionsListOut = new List<CustomFieldsForFilter>();
            List<int> lstCustomFieldId;

            string DropDownList = Convert.ToString(Enums.CustomFieldType.DropDownList);
            string EntityTypeTactic = Convert.ToString(Enums.EntityType.Tactic);
            #endregion

            // Fetch list of Custom Field
            List<CustomField> lstCustomField = objDbMrpEntities.CustomFields.Where(customField => customField.ClientId == ClientId && customField.IsDeleted.Equals(false) &&
                                                                customField.EntityType.Equals(EntityTypeTactic) && customField.CustomFieldType.Name.Equals(DropDownList) &&
                                                                customField.IsDisplayForFilter.Equals(true) && customField.CustomFieldOptions.Count() > 0)
                                                                .Select(customField => customField).ToList();

            if (lstCustomField.Count > 0)
            {
                lstCustomFieldId = lstCustomField.Select(customField => customField.CustomFieldId).Distinct().ToList();

                // Fetch list of Custom Field Options
                List<CustomFieldOption> lstCustomFieldOptions = objDbMrpEntities.CustomFieldOptions
                                                            .Where(customFieldOption => lstCustomFieldId.Contains(customFieldOption.CustomFieldId) && customFieldOption.IsDeleted == false)
                                                            .Select(customFieldOption => customFieldOption).ToList();
                //Permission for Custom field view or edit rights
                bool IsDefaultCustomRestrictionsViewable = Common.IsDefaultCustomRestrictionsViewable();

                // Fetch list of User Custom Restriction
                List<RevenuePlanner.Models.CustomRestriction> userCustomRestrictionList = Common.GetUserCustomRestrictionsList(UserId, true);

                AddCustomfieldBasedonPermission(ref customFieldListOut, ref customFieldOptionsListOut, lstCustomField, lstCustomFieldId, lstCustomFieldOptions, IsDefaultCustomRestrictionsViewable, userCustomRestrictionList);
            }
            if (customFieldListOut.Count() > 0)
            {
                // Ordering List by Title
                customFieldListOut = customFieldListOut.OrderBy(customField => customField.Title).ToList();
            }
            if (customFieldOptionsListOut.Count() > 0)
            {
                // Ordering List by Title
                customFieldOptionsListOut = customFieldOptionsListOut.OrderBy(customFieldOption => customFieldOption.CustomFieldId).ThenBy(customFieldOption => customFieldOption.Title).ToList();
            }
        }

        /// <summary>
        /// Add Custom field Based on Permission
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        public void AddCustomfieldBasedonPermission(ref List<CustomFieldsForFilter> customFieldListOut, ref List<CustomFieldsForFilter> customFieldOptionsListOut, List<CustomField> lstCustomField, List<int> lstCustomFieldId, List<CustomFieldOption> lstCustomFieldOptions, bool IsDefaultCustomRestrictionsViewable, List<RevenuePlanner.Models.CustomRestriction> userCustomRestrictionList)
        {
            if (userCustomRestrictionList.Count() > 0)
            {
                int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
                int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                int NonePermission = (int)Enums.CustomRestrictionPermission.None;

                foreach (int customFieldId in lstCustomFieldId)
                {
                    if (userCustomRestrictionList.Where(customRestriction => customRestriction.CustomFieldId == customFieldId).Count() > 0)
                    {
                        // If user has rights for custom field
                        AddCustomFieldCustomRestrictionBased(customFieldListOut, customFieldOptionsListOut, lstCustomField, lstCustomFieldOptions, IsDefaultCustomRestrictionsViewable, userCustomRestrictionList, ViewOnlyPermission, ViewEditPermission, NonePermission, customFieldId);
                    }
                    else if (IsDefaultCustomRestrictionsViewable)
                    {
                        // If user has no rights for custom field
                        AddCustomfieldOptions(customFieldListOut, customFieldOptionsListOut, lstCustomField, lstCustomFieldOptions, customFieldId);
                    }
                }
            }
            else if (IsDefaultCustomRestrictionsViewable)
            {
                // Add Custom Field in Custom Field List
                customFieldListOut = lstCustomField.Select(customField => new CustomFieldsForFilter()
                {
                    CustomFieldId = customField.CustomFieldId,
                    Title = customField.Name
                }).ToList();

                // Add Custom Field Options in Custom Field Options List
                customFieldOptionsListOut = lstCustomFieldOptions.Select(customFieldOption => new CustomFieldsForFilter()
                {
                    CustomFieldId = customFieldOption.CustomFieldId,
                    CustomFieldOptionId = customFieldOption.CustomFieldOptionId,
                    Title = customFieldOption.Value
                }).ToList();
            }
        }

        /// <summary>
        /// Add Custom field Options
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        public void AddCustomfieldOptions(List<CustomFieldsForFilter> customFieldListOut, List<CustomFieldsForFilter> customFieldOptionsListOut, List<CustomField> lstCustomField, List<CustomFieldOption> lstCustomFieldOptions, int customFieldId)
        {
            // Add Custom Field in Custom Field List
            if (lstCustomFieldOptions.Where(option => option.CustomFieldId == customFieldId).Count() > 0)
            {
                customFieldListOut.AddRange(lstCustomField.Where(customField => customField.CustomFieldId == customFieldId).Select(customField => new CustomFieldsForFilter()
                {
                    CustomFieldId = customField.CustomFieldId,
                    Title = customField.Name
                }).ToList());

                // Add Custom Field Options in Custom Field Options List
                customFieldOptionsListOut.AddRange(lstCustomFieldOptions.Where(option => option.CustomFieldId == customFieldId).Select(customFieldOption => new CustomFieldsForFilter()
                {
                    CustomFieldId = customFieldOption.CustomFieldId,
                    CustomFieldOptionId = customFieldOption.CustomFieldOptionId,
                    Title = customFieldOption.Value
                }).ToList());
            }
        }

        /// <summary>
        /// Add Custom Field Custom Restriction Based
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        public void AddCustomFieldCustomRestrictionBased(List<CustomFieldsForFilter> customFieldListOut, List<CustomFieldsForFilter> customFieldOptionsListOut, List<CustomField> lstCustomField, List<CustomFieldOption> lstCustomFieldOptions, bool IsDefaultCustomRestrictionsViewable, List<RevenuePlanner.Models.CustomRestriction> userCustomRestrictionList, int ViewOnlyPermission, int ViewEditPermission, int NonePermission, int customFieldId)
        {
            // Fetch list of allowed Custom Field Option Ids
            List<int> lstAllowedCustomFieldOptionIds = userCustomRestrictionList.Where(customRestriction => customRestriction.CustomFieldId == customFieldId &&
                                                                                    (customRestriction.Permission == ViewOnlyPermission || customRestriction.Permission == ViewEditPermission))
                                                                                .Select(customRestriction => customRestriction.CustomFieldOptionId).ToList();

            // Fetch list of Restricted Custom Field Options
            List<int> lstRestrictedCustomFieldOptionIds = userCustomRestrictionList.Where(customRestriction => customRestriction.CustomFieldId == customFieldId &&
                                                                                            customRestriction.Permission == NonePermission)
                                                                                    .Select(customRestriction => customRestriction.CustomFieldOptionId).ToList();

            // Fetch list of allowed Custom Field Options
            List<CustomFieldOption> lstAllowedCustomFieldOption = lstCustomFieldOptions.Where(customFieldOption => customFieldOption.CustomFieldId == customFieldId &&
                                                                        lstAllowedCustomFieldOptionIds.Contains(customFieldOption.CustomFieldOptionId))
                                                                    .Select(customFieldOption => new CustomFieldOption
                                                                    {
                                                                        CustomFieldId = customFieldOption.CustomFieldId,
                                                                        CustomFieldOptionId = customFieldOption.CustomFieldOptionId,
                                                                        Value = customFieldOption.Value
                                                                    }).ToList();

            if (lstAllowedCustomFieldOption.Count > 0)
            {
                // Add Custom Field in Custom Field List
                customFieldListOut.AddRange(lstCustomField.Where(customField => customField.CustomFieldId == customFieldId)
                                                        .Select(customField => new CustomFieldsForFilter()
                                                        {
                                                            CustomFieldId = customField.CustomFieldId,
                                                            Title = customField.Name
                                                        }).ToList());

                // Add Custom Field Options in Custom Field Options List
                customFieldOptionsListOut.AddRange(lstAllowedCustomFieldOption.Select(customFieldOption => new CustomFieldsForFilter()
                {
                    CustomFieldId = customFieldOption.CustomFieldId,
                    CustomFieldOptionId = customFieldOption.CustomFieldOptionId,
                    Title = customFieldOption.Value
                }).ToList());

            }

            if (IsDefaultCustomRestrictionsViewable)
            {
                // Add Custom Field in Custom Field List
                List<CustomFieldOption> lstNewCustomFieldOptions = lstCustomFieldOptions.Where(option => !lstAllowedCustomFieldOptionIds.Contains(option.CustomFieldOptionId) && !lstRestrictedCustomFieldOptionIds.Contains(option.CustomFieldOptionId) && option.CustomFieldId == customFieldId).ToList();
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

                    // Add Custom Field Options in Custom Field Options List
                    customFieldOptionsListOut.AddRange(lstNewCustomFieldOptions.Select(customFieldOption => new CustomFieldsForFilter()
                    {
                        CustomFieldId = customFieldOption.CustomFieldId,
                        CustomFieldOptionId = customFieldOption.CustomFieldOptionId,
                        Title = customFieldOption.Value
                    }).ToList());
                }
            }
        }

        /// <summary>
        /// Get TacticType List For Filter
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <returns>returns List of TacticTypeModel</returns>
        public List<TacticTypeModel> GetTacticTypeListForFilter(string PlanId, int UserId, int ClientId)
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

        /// <summary>
        /// Get TacticType List
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <returns>returns List of TacticTypeModel</returns>
        public List<TacticTypeModel> GetTacticTypeList(List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstAllowedEntityIds, int UserId)
        {
            List<Plan_Campaign_Program_Tactic> TacticUserList = tacticList;
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
        /// <returns>returns List of Owner</returns>
        public List<OwnerModel> GetOwnerListForFilter(int ClientId, int UserId, string FirstName, string LastName, Guid ApplicationId, string PlanId, string ViewBy, string ActiveMenu)
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
            List<CacheCustomField> lstAllTacticCustomFieldEntitiesanony = objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => customfieldidlist.Contains(customFieldEntity.CustomFieldId))
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

        /// <summary>
        /// Get Owner List
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
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
        /// <returns>returns List of Individuals By PlanId</returns>
        public List<User> GetIndividualsByPlanId(string ViewBy, string ActiveMenu, List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstAllowedEntityIds, Guid ApplicationId, int UserId)
        {
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
        /// <returns>returns List of PlanId</returns>
        public List<Plan_UserSavedViews> SaveLasSetofViews(string planId, string ViewName, string ownerIds, string TacticTypeid, string StatusIds, string SelectedYears, string customFieldIds, string ParentCustomFieldsIds, List<int> planIds, List<Plan_UserSavedViews> prevCustomFieldList, int UserId)
        {
            List<Plan_UserSavedViews> NewCustomFieldData = new List<Plan_UserSavedViews>();
            #region "Save filter values to Plan_UserSavedViews"
            NewCustomFieldData = SaveFilterValues(planId, ViewName, ownerIds, TacticTypeid, StatusIds, SelectedYears, planIds, UserId);

            #region Variables
            string[] filteredCustomFields = { }; //Array of Combination Custom Field Ids and Custom Field Option Ids
            string PrefixCustom = "CF_"; // String for prefix of Custom Field Id
            StringBuilder FilterName = new StringBuilder(); // String builder to store Existing Filter Values
            #endregion

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
                    SaveCustomFilterValues(ViewName, NewCustomFieldData, filteredCustomFields, PrefixCustom, FilterName, prevCustomFieldList, UserId);
                }
            }
            else
            {
                SaveCustomFieldwithEmptyOptions(ViewName, ParentCustomFieldsIds, NewCustomFieldData, filteredCustomFields, PrefixCustom, UserId);
            }

            if (StatusIds != null && StatusIds.ToLower() == Convert.ToString(Enums.ActiveMenu.Report).ToLower())
            {
                List<Plan_UserSavedViews> statulist = prevCustomFieldList.Where(a => a.FilterName == Enums.FilterLabel.Status.ToString()).ToList();
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
        /// Save Empty Custom Filter Values
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <returns>Array of Filter Values</returns>
        private void SaveCustomFieldwithEmptyOptions(string ViewName, string ParentCustomFieldsIds, List<Plan_UserSavedViews> NewCustomFieldData, string[] filteredCustomFields, string PrefixCustom, int UserId)
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
                    objFilterValues = new Plan_UserSavedViews();
                    objFilterValues.ViewName = null;
                    if (!string.IsNullOrEmpty(ViewName))
                    {
                        objFilterValues.ViewName = ViewName;
                    }
                    objFilterValues.FilterName = PrefixCustom + customField;
                    objFilterValues.FilterValues = string.Empty;
                    objFilterValues.Userid = UserId;
                    objFilterValues.LastModifiedDate = DateTime.Now;
                    objFilterValues.IsDefaultPreset = false;
                    objDbMrpEntities.Plan_UserSavedViews.Add(objFilterValues);
                    NewCustomFieldData.Add(objFilterValues);
                }
            }
        }

        /// <summary>
        /// Save Custom Filter Values
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <returns>Save Custom Filter Values</returns>
        private void SaveCustomFilterValues(string ViewName, List<Plan_UserSavedViews> NewCustomFieldData, string[] filteredCustomFields, string PrefixCustom, StringBuilder FilterName, List<Plan_UserSavedViews> prevCustomFieldList, int UserId)
        {
            string[] filterValues = { };
            string Previousval = string.Empty; // Custom Filed Value without Prefix
            string PreviousValue = string.Empty; // Custom Filed Value with Prefix
            string CustomOptionvalue = string.Empty; // Custom Option Value
            Plan_UserSavedViews ExistingFieldlist = new Plan_UserSavedViews();
            Plan_UserSavedViews objFilterValues = new Plan_UserSavedViews();
            List<Plan_UserSavedViews> listLineitem = prevCustomFieldList;
            foreach (string customField in filteredCustomFields)
            {
                filterValues = customField.Split('_');
                if (filterValues.Count() > 1)
                {
                    CustomOptionvalue = filterValues[1];
                }
                if (filterValues.Count() > 0)
                {
                    string PrevVal = PrefixCustom + Convert.ToString(filterValues[0]);
                    PreviousValue = PrevVal;
                    if (listLineitem != null)
                    {
                        ExistingFieldlist = listLineitem.Where(pcpobjw => pcpobjw.FilterName.Equals(PrevVal)).FirstOrDefault();
                    }

                    if (FilterName.Length == 0)
                    {
                        if (ExistingFieldlist != null)
                        {
                            FilterName.Append(ExistingFieldlist.FilterValues);
                        }
                    }

                    if (FilterName != null && Convert.ToString(FilterName) == PreviousValue && !string.IsNullOrEmpty(CustomOptionvalue))
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
                        objFilterValues.Userid = UserId;
                        objFilterValues.LastModifiedDate = DateTime.Now;
                        objFilterValues.IsDefaultPreset = false;
                        FilterName.Length = 0;
                        FilterName.Append(PrefixCustom + Convert.ToString(filterValues[0]));
                        Previousval = CustomOptionvalue;
                        objDbMrpEntities.Plan_UserSavedViews.Add(objFilterValues);
                    }

                }
                NewCustomFieldData.Add(objFilterValues);
            }
        }

        /// <summary>
        /// Save Filter Values
        /// Modified By Nandish Shah PL Ticket#2611
        /// </summary>
        /// <returns>Returns List of Saved Views</returns>
        private List<Plan_UserSavedViews> SaveFilterValues(string planId, string ViewName, string ownerIds, string TacticTypeid, string StatusIds, string SelectedYears, List<int> planIds, int UserId)
        {
            List<Plan_UserSavedViews> NewCustomFieldData = new List<Plan_UserSavedViews>();
            if (planIds.Count != 0)
            {
                InsertLastViewedUserData(planId, ViewName, Convert.ToString(Enums.FilterLabel.Plan), UserId, NewCustomFieldData);
            }
            InsertLastViewedUserData(ownerIds, ViewName, Convert.ToString(Enums.FilterLabel.Owner), UserId, NewCustomFieldData);
            InsertLastViewedUserData(TacticTypeid, ViewName, Convert.ToString(Enums.FilterLabel.TacticType), UserId, NewCustomFieldData);
            if (StatusIds != null && StatusIds.ToLower() != Convert.ToString(Enums.ActiveMenu.Report).ToLower())
            {
                InsertLastViewedUserData(StatusIds, ViewName, Convert.ToString(Enums.FilterLabel.Status), UserId, NewCustomFieldData);
            }
            if (!string.IsNullOrEmpty(SelectedYears))
            {
                InsertLastViewedUserData(SelectedYears, ViewName, Convert.ToString(Enums.FilterLabel.Year), UserId, NewCustomFieldData);
            }
            return NewCustomFieldData;
        }

        /// <summary>
        /// Added By: Nandish Shah.
        /// Desc : Function to insert data into Plan_UserSaved Views table for filters in left pane.
        /// </summary>
        private void InsertLastViewedUserData(string Ids, string ViewName, string FilterName, int UserId, List<Plan_UserSavedViews> NewCustomFieldData)
        {
            Plan_UserSavedViews objFilterValues = new Plan_UserSavedViews();
            objFilterValues.ViewName = null;
            if (!string.IsNullOrEmpty(ViewName))
            {
                objFilterValues.ViewName = ViewName;
            }
            objFilterValues.FilterName = FilterName;
            objFilterValues.FilterValues = Ids;
            objFilterValues.Userid = UserId;
            objFilterValues.LastModifiedDate = DateTime.Now;
            objFilterValues.IsDefaultPreset = false;
            NewCustomFieldData.Add(objFilterValues);
            objDbMrpEntities.Entry(objFilterValues).State = EntityState.Added;
        }

        /// <summary>
        /// Added By: Nandish Shah.
        /// Desc : Function to get Plan based on selection of Year.
        /// </summary>
        public List<SelectListItem> GetPlanBasedOnYear(string Year, int ClientId)
        {
            string[] ListYears = { };
            if (!string.IsNullOrEmpty(Year))
            {
                ListYears = Year.Split(',');
            }
            int Firstyear = 0;
            List<SelectListItem> planList = new List<SelectListItem>();
            if (ListYears != null && ListYears.Length > 0 && int.TryParse(ListYears[0], out Firstyear))
            {
                //Get Active Plans for particular Model
                List<Plan> DataPlanList = objDbMrpEntities.Plans.Where(plan => plan.IsDeleted == false && plan.Model.IsDeleted == false && plan.Model.ClientId == ClientId && plan.IsActive == true).ToList();

                //List of distinct PlanIDs
                List<int> uniqueplanids = DataPlanList.Select(p => p.PlanId).Distinct().ToList();

                //List of Plan based on Campaign wise
                List<int> CampPlanIds = objDbMrpEntities.Plan_Campaign.Where(camp => camp.IsDeleted == false && uniqueplanids.Contains(camp.PlanId))
                    .Select(camp => new { PlanId = camp.PlanId, StartDate = camp.StartDate, EndDate = camp.EndDate }).ToList()
                    .Where(camp => ListYears.Contains(camp.StartDate.Year.ToString()) || ListYears.Contains(camp.EndDate.Year.ToString()))
                    .Select(camp => camp.PlanId).Distinct().ToList();

                //List of planIds based on selected years
                List<int> PlanIds = DataPlanList.Where(plan => ListYears.Contains(plan.Year))
                    .Select(plan => plan.PlanId).Distinct().ToList();

                //Get Distinct Campaign wise Plan Ids
                List<int> allPlanIds = CampPlanIds.Concat(PlanIds).Distinct().ToList();

                planList = DataPlanList.Where(plan => allPlanIds.Contains(plan.PlanId)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList().Select(plan => new SelectListItem
                {
                    Text = plan.Title,
                    Value = plan.PlanId.ToString()
                }).ToList();
            }
            planList = planList.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            return planList;
        }

        public string SetDefaultFilterPresetName(int UserID)
        {
            string DefaultPresetName =  objDbMrpEntities.Plan_UserSavedViews.Where(x => x.Userid == UserID && x.IsDefaultPreset == true).Select(x => x.ViewName).FirstOrDefault();
            return DefaultPresetName;
        }
    }
}
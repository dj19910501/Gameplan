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
                objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);
                objCache.AddCache(Enums.CacheObject.PlanTacticListforpackageing.ToString(), customtacticList);
                var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);

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
            var TacticUserList = tacticList.ToList();
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
            var StatusLabel = Enums.FilterLabel.Status.ToString();
            var LastSetOfStatus = new List<string>();
            var NewListOfViews = new List<Plan_UserSavedViews>();
            var listofsavedviews = new List<Plan_UserSavedViews>();
            if (PlanUserSavedViews == null || isLoadPreset == true)
            {
                listofsavedviews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == UserId).Select(view => view).ToList();
                Common.PlanUserSavedViews = listofsavedviews;
            }
            else
            {
                Common.PlanUserSavedViews = PlanUserSavedViews;
                listofsavedviews = Common.PlanUserSavedViews;
            }
            return listofsavedviews;
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
                string section = Enums.Section.Tactic.ToString();

                var customfield = objDbMrpEntities.CustomFields.Where(customField => customField.EntityType == section && customField.ClientId == ClientId && customField.IsDeleted == false).ToList();
                objCache.AddCache(Enums.CacheObject.CustomField.ToString(), customfield);
                var customfieldidlist = customfield.Select(c => c.CustomFieldId).ToList();
                var lstAllTacticCustomFieldEntitiesanony = objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => customfieldidlist.Contains(customFieldEntity.CustomFieldId))
                                                                                       .Select(customFieldEntity => new CacheCustomField { EntityId = customFieldEntity.EntityId, CustomFieldId = customFieldEntity.CustomFieldId, Value = customFieldEntity.Value, CreatedBy = customFieldEntity.CreatedBy, CustomFieldEntityId = customFieldEntity.CustomFieldEntityId }).Distinct().ToList();

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

                    List<int> AllowedEntityIds = Common.GetViewableTacticList(UserId, ClientId, planTacticIds, false, customfieldlist);
                    if (AllowedEntityIds.Count > 0)
                    {
                        lstAllowedEntityIds.AddRange(AllowedEntityIds);
                    }

                }
                var LoggedInUser = new OwnerModel
                {
                    OwnerId = UserId.ToString(),
                    Title = Convert.ToString(FirstName + " " + LastName),
                };

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
            var lstOwners = GetIndividualsByPlanId(ViewBy, ActiveMenu, tacticList, lstAllowedEntityIds, ApplicationId, UserId);
            List<OwnerModel> lstAllowedOwners = lstOwners.Select(owner => new OwnerModel
            {
                OwnerId = Convert.ToString(owner.UserId),
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
            var TacticUserList = tacticList.Distinct().ToList();
            if (TacticUserList.Count > 0)
            {
                TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == UserId).ToList();
            }
            var useridslist = TacticUserList.Select(tactic => tactic.CreatedBy).Distinct().ToList();
            var individuals = bdsUserRepository.GetMultipleTeamMemberNameByApplicationIdEx(useridslist, ApplicationId);
            return individuals;
        }
    }
}
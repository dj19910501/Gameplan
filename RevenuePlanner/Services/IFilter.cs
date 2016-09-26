using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RevenuePlanner.Services
{
    public interface IFilter
    {
        HomePlanModel GetFilterData(List<int> currentPlanId, int UserId, int ClientId, List<int> LstPlanId, string FilterPresetName, ref List<Plan_UserSavedViews> PlanUserSavedViews, ref List<SelectListItem> LstYear);
        List<TacticTypeModel> GetTacticTypeListForFilter(string PlanId, int UserId, int ClientId);
        List<Plan_UserSavedViews> LastSetOfViews(int UserId, List<Plan_UserSavedViews> PlanUserSavedViews, string PresetName = "", Boolean isLoadPreset = false);
        List<Preset> GetListofPreset(List<Plan_UserSavedViews> listofsavedviews, string PresetName = "");
        List<OwnerModel> GetOwnerListForFilter(int ClientId, int UserId, string FirstName, string LastName, Guid ApplicationId, string PlanId, string ViewBy, string ActiveMenu);
        List<Plan_UserSavedViews> SaveLasSetofViews(string planId, string ViewName, string ownerIds, string TacticTypeid, string StatusIds, string SelectedYears, string customFieldIds, string ParentCustomFieldsIds, List<int> planIds, List<Plan_UserSavedViews> prevCustomFieldList, int UserId);
        List<int> GetPlanIds(string planId);
    }
}

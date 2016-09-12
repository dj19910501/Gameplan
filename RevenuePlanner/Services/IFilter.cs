using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevenuePlanner.Services
{
    public interface IFilter
    {
        List<TacticTypeModel> GetTacticTypeListForFilter(string PlanId, Guid UserId, Guid ClientId);
        List<Plan_UserSavedViews> LastSetOfViews(Guid UserId, List<Plan_UserSavedViews> PlanUserSavedViews, string PresetName = "", Boolean isLoadPreset = false);
        List<Preset> GetListofPreset(List<Plan_UserSavedViews> listofsavedviews, string PresetName = "");
        List<OwnerModel> GetOwnerListForFilter(Guid ClientId, Guid UserId, string FirstName, string LastName, Guid ApplicationId, string PlanId, string ViewBy, string ActiveMenu);
    }
}

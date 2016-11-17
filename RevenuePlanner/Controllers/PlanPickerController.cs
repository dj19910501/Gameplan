using System.Collections.Generic;
using System.Web.Http;
using RevenuePlanner.Helpers;
using RevenuePlanner.Services.PlanPicker;

namespace RevenuePlanner.Controllers
{
    public class PlanPickerController : ApiController
    {
        private IPlanPicker _planPicker; 
        public PlanPickerController(IPlanPicker planPicker)
        {
            _planPicker = planPicker;
        }
        public List<PlanItem> GetCampaigns(int planId)
        {
            return _planPicker.GetCampaigns(planId);
        }

        public List<PlanItem> GetLineItems(int tacticId)
        {
            return _planPicker.GetLineItems(tacticId);
        }

        public List<PlanItem> GetPlans(int year)
        {
            return _planPicker.GetPlans(Sessions.User.CID, year);
        }

        public List<PlanItem> GetPrograms(int campaignId)
        {
            return _planPicker.GetPrograms(campaignId);
        }

        public List<PlanItem> GetTatics(int programId)
        {
            return _planPicker.GetTatics(programId);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using RevenuePlanner.Helpers;
using RevenuePlanner.Services.PlanPicker;

namespace RevenuePlanner.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class PlanPickerController : ApiController
    {
        private IPlanPicker _planPicker; 
        public PlanPickerController(IPlanPicker planPicker)
        {
            _planPicker = planPicker;
        }
        public IEnumerable<PlanItem> GetCampaigns(int planId)
        {
            return _planPicker.GetCampaigns(planId);
        }

        public IEnumerable<PlanItem> GetLineItems(int tacticId)
        {
            return _planPicker.GetLineItems(tacticId);
        }

        public IEnumerable<PlanItem> GetPlans(string year)
        {
            return _planPicker.GetPlans(Sessions.User.CID, year);
        }

        public IEnumerable<PlanItem> GetPrograms(int campaignId)
        {
            return _planPicker.GetPrograms(campaignId);
        }

        public IEnumerable<PlanItem> GetTactics(int programId)
        {
            return _planPicker.GetTactics(programId);
        }
        
        public IEnumerable<String> GetYears()
        {
            return _planPicker.GetYears(Sessions.User.CID);
        }
    }
}
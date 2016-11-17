using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Services.PlanPicker
{
    public class PlanPicker : IPlanPicker
    {
        public List<PlanItem> GetCampaigns(int planId)
        {
            throw new NotImplementedException();
        }

        public List<PlanItem> GetLineItems(int tacticId)
        {
            throw new NotImplementedException();
        }

        public List<PlanItem> GetPlans(int clientId, int year)
        {
            throw new NotImplementedException();
        }

        public List<PlanItem> GetPrograms(int campaignId)
        {
            throw new NotImplementedException();
        }

        public List<PlanItem> GetTatics(int programId)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevenuePlanner.Services.PlanPicker
{
    public class PlanItem {
        public int Id { get; set; }
        public string Title { get; set; }
    } 

    public interface IPlanPicker
    {
        List<PlanItem> GetPlans(int clientId, int year);
        List<PlanItem> GetCampaigns(int planId);
        List<PlanItem> GetPrograms(int campaignId);
        List<PlanItem> GetTatics(int programId);
        List<PlanItem> GetLineItems(int tacticId);
    }
}

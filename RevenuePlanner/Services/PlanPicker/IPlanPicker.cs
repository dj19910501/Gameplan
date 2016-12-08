using System.Collections.Generic;

namespace RevenuePlanner.Services.PlanPicker
{
    public class PlanItem {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class PlanItemWithCost: PlanItem
    {
        public double Cost { get; set; }
    }

    public interface IPlanPicker
    {

        List<string> GetYears(int clientId);
        List<PlanItem> GetPlans(int clientId, string year);
        List<PlanItem> GetCampaigns(int planId);
        List<PlanItem> GetPrograms(int campaignId);
        List<PlanItem> GetTactics(int programId);
        List<PlanItemWithCost> GetLineItems(int tacticId);
    }
}

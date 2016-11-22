using System;
using System.Collections.Generic;
using RevenuePlanner.Models;
using System.Linq;

namespace RevenuePlanner.Services.PlanPicker
{
    public class PlanPicker : IPlanPicker
    {
        private MRPEntities _database;
        public PlanPicker(MRPEntities database)
        {
            _database = database;
        }

        public List<PlanItem> GetCampaigns(int planId)
        {
            var sqlQuery =
                from campaign in _database.Plan_Campaign
                where campaign.PlanId == planId
                select new PlanItem { Id = campaign.PlanCampaignId, Title = campaign.Title };
            return sqlQuery.ToList();
        }

        public List<PlanItem> GetLineItems(int tacticId)
        {
            var sqlQuery =
                from lineItem in _database.Plan_Campaign_Program_Tactic_LineItem
                where lineItem.PlanTacticId == tacticId
                select new PlanItem { Id = lineItem.PlanLineItemId, Title = lineItem.Title };
            return sqlQuery.ToList();
        }

        public List<PlanItem> GetPlans(int clientId, string year)
        {
            var innerJoinQuery =
                from plan in _database.Plans
                join model in _database.Models on plan.ModelId equals model.ModelId
                where model.ClientId == clientId && plan.Year == year
                select new PlanItem { Id= plan.PlanId, Title = plan.Title }; 
            return innerJoinQuery.ToList();
        }

        public List<PlanItem> GetPrograms(int campaignId)
        {
            var sqlQuery =
                from program in _database.Plan_Campaign_Program
                where program.PlanCampaignId == campaignId
                select new PlanItem { Id = program.PlanProgramId, Title = program.Title };
            return sqlQuery.ToList();
        }

        public List<PlanItem> GetTatics(int programId)
        {
            var sqlQuery =
                from tactic in _database.Plan_Campaign_Program_Tactic
                where tactic.PlanProgramId == programId
                select new PlanItem { Id = tactic.PlanTacticId, Title = tactic.Title };
            return sqlQuery.ToList();
        }
    }
}
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
                where campaign.PlanId == planId && campaign.IsDeleted == false
                select new PlanItem { Id = campaign.PlanCampaignId, Title = campaign.Title };
            return sqlQuery.ToList();
        }

        public List<PlanItemWithCost> GetLineItems(int tacticId)
        {
            var sqlQuery =
                from lineItem in _database.Plan_Campaign_Program_Tactic_LineItem
                where lineItem.PlanTacticId == tacticId && lineItem.Title.ToLower() != "sys_gen_balance" && lineItem.IsDeleted == false
                select new PlanItemWithCost { Id = lineItem.PlanLineItemId, Title = lineItem.Title, Cost = lineItem.Cost };
            return sqlQuery.ToList();
        }

        public List<PlanItem> GetPlans(int clientId, string year)
        {
            var innerJoinQuery =
                from plan in _database.Plans
                join model in _database.Models on plan.ModelId equals model.ModelId
                where model.ClientId == clientId && plan.Year == year && plan.IsDeleted == false
                select new PlanItem { Id= plan.PlanId, Title = plan.Title }; 
            return innerJoinQuery.ToList();
        }

        public List<PlanItem> GetPrograms(int campaignId)
        {
            var sqlQuery =
                from program in _database.Plan_Campaign_Program
                where program.PlanCampaignId == campaignId && program.IsDeleted == false
                select new PlanItem { Id = program.PlanProgramId, Title = program.Title };
            return sqlQuery.ToList();
        }

        public List<PlanItem> GetTactics(int programId)
        {
            var sqlQuery =
                from tactic in _database.Plan_Campaign_Program_Tactic
                where tactic.PlanProgramId == programId && tactic.IsDeleted == false
                select new PlanItem { Id = tactic.PlanTacticId, Title = tactic.Title };
            return sqlQuery.ToList();
        }

        public List<String> GetYears(int clientId)
        {
            IEnumerable<String> sqlQuery =
                from plan in _database.Plans
                join model in _database.Models on plan.ModelId equals model.ModelId
                where model.ClientId == clientId && model.IsDeleted == false
                group plan by plan.Year into year
                orderby year.Key
                select year.Key;

            return sqlQuery.ToList();
        }
    }
}
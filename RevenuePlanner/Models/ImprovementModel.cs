using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class PlanImprovementCampaign
    {
        public int ImprovePlanId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EffectiveDate { get; set; }
        public double? Cost { get; set; }
    }

    public class PlanImprovementTactic
    {
        public int ImprovementPlanTacticId { get; set; }
        public int ImprovementPlanProgramId { get; set; }
        public int ImprovementTacticTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EffectiveDate { get; set; }
        public double Cost { get; set; }
        public string Status { get; set; }

        public bool IsDeployedToIntegration { get; set; }
    }

    public class ImprovementStage
    {
        public int StageId { get; set; }
        public string StageCode { get; set; }
        public string StageName { get; set; }
        public string StageType { get; set; }
        public double BaseLineRate { get; set; }
        public double PlanWithoutTactic { get; set; }
        public double PlanWithTactic { get; set; }
        public int ClientId { get; set; }
    }
}
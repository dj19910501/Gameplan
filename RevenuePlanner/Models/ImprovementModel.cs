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
        public int? VerticalId { get; set; }
        public int? AudienceId { get; set; }
        public Guid? GeographyId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public double? Cost { get; set; }
    }

    public class PlanImprovementTactic
    {
        public int ImprovementPlanProgramId { get; set; }
        public int ImprovementTacticTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? VerticalId { get; set; }
        public int? AudienceId { get; set; }
        public Guid? GeographyId { get; set; }
        public Guid BusinessUnitId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public double Cost { get; set; }
        public double? CostActual { get; set; }
        public string Status { get; set; }
    }
}
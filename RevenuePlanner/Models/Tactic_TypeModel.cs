using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace RevenuePlanner.Models
{
    public class Tactic_TypeModel
    {
        [Key]
        public int TacticTypeId { get; set; }
        public int ClientId { get; set; }
        public int? ModelId { get; set; }
        public int? StageId { get; set; }
        //changes done by uday for #497
        public Nullable<double> ProjectedStageValue { get; set; }
        public Nullable<double> ProjectedRevenue { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public int ModifiedBy { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public List<ModelVersion> Versions { get; set; }
        public bool IsDeployedToIntegration { get; set; }
        public int? WorkFrontTemplateId { get; set; } //updated 1/7/2016 by Brad Gray PL#1856
        public string programType { get; set; }
        public string Channel { get; set; }
        public string AssetType { get; set; }
    }
}
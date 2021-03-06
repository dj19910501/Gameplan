//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RevenuePlanner.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Plan_Improvement_Campaign_Program_Tactic
    {
        public Plan_Improvement_Campaign_Program_Tactic()
        {
            this.Plan_Improvement_Campaign_Program_Tactic_Comment = new HashSet<Plan_Improvement_Campaign_Program_Tactic_Comment>();
            this.Plan_Improvement_Campaign_Program_Tactic_Share = new HashSet<Plan_Improvement_Campaign_Program_Tactic_Share>();
        }
    
        public int ImprovementPlanTacticId { get; set; }
        public int ImprovementPlanProgramId { get; set; }
        public int ImprovementTacticTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.DateTime EffectiveDate { get; set; }
        public double Cost { get; set; }
        public string Status { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool IsDeployedToIntegration { get; set; }
        public string IntegrationInstanceTacticId { get; set; }
        public Nullable<System.DateTime> LastSyncDate { get; set; }
        public string IntegrationInstanceEloquaId { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
    
        public virtual ImprovementTacticType ImprovementTacticType { get; set; }
        public virtual Plan_Improvement_Campaign_Program Plan_Improvement_Campaign_Program { get; set; }
        public virtual ICollection<Plan_Improvement_Campaign_Program_Tactic_Comment> Plan_Improvement_Campaign_Program_Tactic_Comment { get; set; }
        public virtual ICollection<Plan_Improvement_Campaign_Program_Tactic_Share> Plan_Improvement_Campaign_Program_Tactic_Share { get; set; }
    }
}

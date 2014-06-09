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
    
    public partial class Plan_Campaign_Program_Tactic
    {
        public Plan_Campaign_Program_Tactic()
        {
            this.Plan_Campaign_Program_Tactic_Actual = new HashSet<Plan_Campaign_Program_Tactic_Actual>();
            this.Plan_Campaign_Program_Tactic_Comment = new HashSet<Plan_Campaign_Program_Tactic_Comment>();
            this.Tactic_Share = new HashSet<Tactic_Share>();
        }
    
        public int PlanTacticId { get; set; }
        public int PlanProgramId { get; set; }
        public int TacticTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int VerticalId { get; set; }
        public int AudienceId { get; set; }
        public System.Guid GeographyId { get; set; }
        public System.Guid BusinessUnitId { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public double Cost { get; set; }
        public Nullable<double> CostActual { get; set; }
        public string Status { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDeployedToIntegration { get; set; }
        public string IntegrationInstanceTacticId { get; set; }
        public Nullable<System.DateTime> LastSyncDate { get; set; }
        public Nullable<double> ProjectedStageValue { get; set; }
        public int StageId { get; set; }
    
        public virtual Audience Audience { get; set; }
        public virtual BusinessUnit BusinessUnit { get; set; }
        public virtual Geography Geography { get; set; }
        public virtual Plan_Campaign_Program Plan_Campaign_Program { get; set; }
        public virtual ICollection<Plan_Campaign_Program_Tactic_Actual> Plan_Campaign_Program_Tactic_Actual { get; set; }
        public virtual ICollection<Plan_Campaign_Program_Tactic_Comment> Plan_Campaign_Program_Tactic_Comment { get; set; }
        public virtual Plan_Campaign_Program_Tactic Plan_Campaign_Program_Tactic1 { get; set; }
        public virtual Plan_Campaign_Program_Tactic Plan_Campaign_Program_Tactic2 { get; set; }
        public virtual Stage Stage { get; set; }
        public virtual TacticType TacticType { get; set; }
        public virtual Vertical Vertical { get; set; }
        public virtual ICollection<Tactic_Share> Tactic_Share { get; set; }
    }
}

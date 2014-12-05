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
    
    public partial class Plan_Campaign
    {
        public Plan_Campaign()
        {
            this.Plan_Campaign_Program_Tactic_Comment = new HashSet<Plan_Campaign_Program_Tactic_Comment>();
            this.Tactic_Share = new HashSet<Tactic_Share>();
            this.Plan_Campaign_Budget = new HashSet<Plan_Campaign_Budget>();
            this.Plan_Campaign_Program = new HashSet<Plan_Campaign_Program>();
        }
    
        public int PlanCampaignId { get; set; }
        public int PlanId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Nullable<int> VerticalId { get; set; }
        public Nullable<int> AudienceId { get; set; }
        public Nullable<System.Guid> GeographyId { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public string Status { get; set; }
        public bool IsDeployedToIntegration { get; set; }
        public string IntegrationInstanceCampaignId { get; set; }
        public Nullable<System.DateTime> LastSyncDate { get; set; }
        public double CampaignBudget { get; set; }
        public string Abbreviation { get; set; }
    
        public virtual Audience Audience { get; set; }
        public virtual Geography Geography { get; set; }
        public virtual Plan Plan { get; set; }
        public virtual ICollection<Plan_Campaign_Program_Tactic_Comment> Plan_Campaign_Program_Tactic_Comment { get; set; }
        public virtual ICollection<Tactic_Share> Tactic_Share { get; set; }
        public virtual ICollection<Plan_Campaign_Budget> Plan_Campaign_Budget { get; set; }
        public virtual ICollection<Plan_Campaign_Program> Plan_Campaign_Program { get; set; }
        public virtual Vertical Vertical { get; set; }
    }
}

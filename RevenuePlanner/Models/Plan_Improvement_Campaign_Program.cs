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
    
    public partial class Plan_Improvement_Campaign_Program
    {
        public Plan_Improvement_Campaign_Program()
        {
            this.Plan_Improvement_Campaign_Program_Tactic = new HashSet<Plan_Improvement_Campaign_Program_Tactic>();
        }
    
        public int ImprovementPlanProgramId { get; set; }
        public int ImprovementPlanCampaignId { get; set; }
        public string Title { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
    
        public virtual Plan_Improvement_Campaign Plan_Improvement_Campaign { get; set; }
        public virtual ICollection<Plan_Improvement_Campaign_Program_Tactic> Plan_Improvement_Campaign_Program_Tactic { get; set; }
    }
}

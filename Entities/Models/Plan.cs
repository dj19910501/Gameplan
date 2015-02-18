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
    
    public partial class Plan
    {
        public Plan()
        {
            this.Plan_Budget = new HashSet<Plan_Budget>();
            this.Plan_Team = new HashSet<Plan_Team>();
            this.Plan_Improvement_Campaign = new HashSet<Plan_Improvement_Campaign>();
            this.Plan_Campaign = new HashSet<Plan_Campaign>();
        }
    
        public int PlanId { get; set; }
        public int ModelId { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string Year { get; set; }
        public string Description { get; set; }
        public double Budget { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public string GoalType { get; set; }
        public double GoalValue { get; set; }
        public string AllocatedBy { get; set; }
        public string EloquaFolderPath { get; set; }
    
        public virtual Model Model { get; set; }
        public virtual ICollection<Plan_Budget> Plan_Budget { get; set; }
        public virtual ICollection<Plan_Team> Plan_Team { get; set; }
        public virtual ICollection<Plan_Improvement_Campaign> Plan_Improvement_Campaign { get; set; }
        public virtual ICollection<Plan_Campaign> Plan_Campaign { get; set; }
    }
}

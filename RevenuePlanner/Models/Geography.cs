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
    
    public partial class Geography
    {
        public Geography()
        {
            this.Plan_Campaign = new HashSet<Plan_Campaign>();
            this.Plan_Campaign_Program = new HashSet<Plan_Campaign_Program>();
            this.Plan_Campaign_Program_Tactic = new HashSet<Plan_Campaign_Program_Tactic>();
            this.Plan_Improvement_Campaign = new HashSet<Plan_Improvement_Campaign>();
            this.Plan_Improvement_Campaign_Program = new HashSet<Plan_Improvement_Campaign_Program>();
            this.Plan_Improvement_Campaign_Program_Tactic = new HashSet<Plan_Improvement_Campaign_Program_Tactic>();
        }
    
        public System.Guid GeographyId { get; set; }
        public string Title { get; set; }
        public System.Guid ClientId { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
    
        public virtual ICollection<Plan_Campaign> Plan_Campaign { get; set; }
        public virtual ICollection<Plan_Campaign_Program> Plan_Campaign_Program { get; set; }
        public virtual ICollection<Plan_Campaign_Program_Tactic> Plan_Campaign_Program_Tactic { get; set; }
        public virtual ICollection<Plan_Improvement_Campaign> Plan_Improvement_Campaign { get; set; }
        public virtual ICollection<Plan_Improvement_Campaign_Program> Plan_Improvement_Campaign_Program { get; set; }
        public virtual ICollection<Plan_Improvement_Campaign_Program_Tactic> Plan_Improvement_Campaign_Program_Tactic { get; set; }
    }
}

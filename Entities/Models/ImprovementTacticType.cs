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
    
    public partial class ImprovementTacticType
    {
        public ImprovementTacticType()
        {
            this.ImprovementTacticType_Metric = new HashSet<ImprovementTacticType_Metric>();
            this.Plan_Improvement_Campaign_Program_Tactic = new HashSet<Plan_Improvement_Campaign_Program_Tactic>();
        }
    
        public int ImprovementTacticTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Cost { get; set; }
        public string ColorCode { get; set; }
        public System.Guid ClientId { get; set; }
        public bool IsDeployed { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public bool IsDeployedToIntegration { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
    
        public virtual ICollection<ImprovementTacticType_Metric> ImprovementTacticType_Metric { get; set; }
        public virtual ICollection<Plan_Improvement_Campaign_Program_Tactic> Plan_Improvement_Campaign_Program_Tactic { get; set; }
        public virtual ImprovementTacticType ImprovementTacticType1 { get; set; }
        public virtual ImprovementTacticType ImprovementTacticType2 { get; set; }
    }
}

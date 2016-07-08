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
    
    public partial class TacticType
    {
        public TacticType()
        {
            this.Plan_Campaign_Program_Tactic = new HashSet<Plan_Campaign_Program_Tactic>();
        }
    
        public int TacticTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ColorCode { get; set; }
        public int ModelId { get; set; }
        public Nullable<int> StageId { get; set; }
        public Nullable<double> ProjectedRevenue { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<int> PreviousTacticTypeId { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public bool IsDeployedToIntegration { get; set; }
        public Nullable<double> ProjectedStageValue { get; set; }
        public bool IsDeployedToModel { get; set; }
        public string Abbreviation { get; set; }
        public Nullable<int> WorkFrontTemplateId { get; set; }
        public string AssetType { get; set; }
    
        public virtual IntegrationWorkFrontTemplate IntegrationWorkFrontTemplate { get; set; }
        public virtual Model Model { get; set; }
        public virtual ICollection<Plan_Campaign_Program_Tactic> Plan_Campaign_Program_Tactic { get; set; }
        public virtual Stage Stage { get; set; }
    }
}

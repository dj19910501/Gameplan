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
    
    public partial class Tactic_Share
    {
        public int TacticShareId { get; set; }
        public Nullable<int> PlanTacticId { get; set; }
        public string EmailId { get; set; }
        public string EmailBody { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<int> PlanProgramId { get; set; }
        public Nullable<int> PlanCampaignId { get; set; }
    
        public virtual Plan_Campaign Plan_Campaign { get; set; }
        public virtual Plan_Campaign_Program Plan_Campaign_Program { get; set; }
        public virtual Plan_Campaign_Program_Tactic Plan_Campaign_Program_Tactic { get; set; }
    }
}

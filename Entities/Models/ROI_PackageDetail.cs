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
    
    public partial class ROI_PackageDetail
    {
        public int Id { get; set; }
        public int AnchorTacticID { get; set; }
        public int PlanTacticId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
    
        public virtual Plan_Campaign_Program_Tactic Plan_Campaign_Program_Tactic { get; set; }
        public virtual Plan_Campaign_Program_Tactic Plan_Campaign_Program_Tactic1 { get; set; }
    }
}
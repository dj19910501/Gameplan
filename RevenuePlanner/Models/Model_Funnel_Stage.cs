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
    
    public partial class Model_Funnel_Stage
    {
        public int ModelFunnelStageId { get; set; }
        public int ModelFunnelId { get; set; }
        public int StageId { get; set; }
        public string StageType { get; set; }
        public double Value { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
    
        public virtual Model_Funnel Model_Funnel { get; set; }
        public virtual Stage Stage { get; set; }
    }
}

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
    
    public partial class ImprovementTacticType_Metric
    {
        public int ImprovementTacticTypeId { get; set; }
        public int MetricId { get; set; }
        public double Weight { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
    
        public virtual ImprovementTacticType ImprovementTacticType { get; set; }
        public virtual Metric Metric { get; set; }
    }
}

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
    
    public partial class AggregationStep
    {
        public int id { get; set; }
        public string StepName { get; set; }
        public bool LogStart { get; set; }
        public bool LogEnd { get; set; }
        public string QueryText { get; set; }
        public double StepOrder { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public bool isDeleted { get; set; }
        public string StatusCode { get; set; }
        public string PartialQueryText { get; set; }
    }
}
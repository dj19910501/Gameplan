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
    
    public partial class Plan_Campaign_Program_Tactic_LineItem_Actual
    {
        public int PlanLineItemId { get; set; }
        public string Period { get; set; }
        public double Value { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
    
        public virtual Plan_Campaign_Program_Tactic_LineItem Plan_Campaign_Program_Tactic_LineItem { get; set; }
    }
}

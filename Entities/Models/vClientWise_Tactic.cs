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
    
    public partial class vClientWise_Tactic
    {
        public int PlanTacticId { get; set; }
        public string Title { get; set; }
        public int MediaCodeId { get; set; }
        public int TacticId { get; set; }
        public string MediaCodeValue { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<long> MediaCode { get; set; }
        public int CreatedBy { get; set; }
        public int LastModifiedBy { get; set; }
        public int ClientId { get; set; }
    }
}

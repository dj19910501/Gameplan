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
    
    public partial class Alert
    {
        public int AlertId { get; set; }
        public int RuleId { get; set; }
        public string Description { get; set; }
        public bool IsRead { get; set; }
        public System.Guid UserId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime DisplayDate { get; set; }
        public Nullable<System.DateTime> ReadDate { get; set; }
        public string IsEmailSent { get; set; }
    
        public virtual Alert_Rules Alert_Rules { get; set; }
    }
}

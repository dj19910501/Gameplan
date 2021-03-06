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
    
    public partial class CustomFieldOption
    {
        public CustomFieldOption()
        {
            this.CustomFieldDependencies = new HashSet<CustomFieldDependency>();
            this.CustomRestrictions = new HashSet<CustomRestriction>();
        }
    
        public int CustomFieldOptionId { get; set; }
        public int CustomFieldId { get; set; }
        public string Value { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string Abbreviation { get; set; }
        public string Description { get; set; }
        public string ColorCode { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public string CustomFieldOptionIDX { get; set; }
    
        public virtual CustomField CustomField { get; set; }
        public virtual ICollection<CustomFieldDependency> CustomFieldDependencies { get; set; }
        public virtual ICollection<CustomRestriction> CustomRestrictions { get; set; }
    }
}

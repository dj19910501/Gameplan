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
    
    public partial class CustomField
    {
        public CustomField()
        {
            this.CampaignNameConventions = new HashSet<CampaignNameConvention>();
            this.CustomFieldOptions = new HashSet<CustomFieldOption>();
            this.CustomRestrictions = new HashSet<CustomRestriction>();
            this.IntegrationInstanceDataTypeMappings = new HashSet<IntegrationInstanceDataTypeMapping>();
            this.CustomField_Entity = new HashSet<CustomField_Entity>();
            this.CustomFieldDependencies = new HashSet<CustomFieldDependency>();
            this.CustomFieldDependencies1 = new HashSet<CustomFieldDependency>();
        }
    
        public int CustomFieldId { get; set; }
        public string Name { get; set; }
        public int CustomFieldTypeId { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public string EntityType { get; set; }
        public System.Guid ClientId { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public bool IsDisplayForFilter { get; set; }
        public string AbbreviationForMulti { get; set; }
        public bool IsDefault { get; set; }
        public bool IsGet { get; set; }
    
        public virtual ICollection<CampaignNameConvention> CampaignNameConventions { get; set; }
        public virtual CustomFieldType CustomFieldType { get; set; }
        public virtual ICollection<CustomFieldOption> CustomFieldOptions { get; set; }
        public virtual ICollection<CustomRestriction> CustomRestrictions { get; set; }
        public virtual ICollection<IntegrationInstanceDataTypeMapping> IntegrationInstanceDataTypeMappings { get; set; }
        public virtual ICollection<CustomField_Entity> CustomField_Entity { get; set; }
        public virtual ICollection<CustomFieldDependency> CustomFieldDependencies { get; set; }
        public virtual ICollection<CustomFieldDependency> CustomFieldDependencies1 { get; set; }
    }
}

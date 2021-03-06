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
    
    public partial class IntegrationInstanceDataTypeMapping
    {
        public int IntegrationInstanceDataTypeMappingId { get; set; }
        public int IntegrationInstanceId { get; set; }
        public Nullable<int> GameplanDataTypeId { get; set; }
        public string TargetDataType { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<int> CustomFieldId { get; set; }
        public int CreatedBy { get; set; }
    
        public virtual CustomField CustomField { get; set; }
        public virtual GameplanDataType GameplanDataType { get; set; }
        public virtual IntegrationInstance IntegrationInstance { get; set; }
    }
}

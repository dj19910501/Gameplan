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
    
    public partial class IntegrationInstanceDataTypeMappingPull
    {
        public int IntegrationInstanceDataTypeMappingPullId { get; set; }
        public int IntegrationInstanceId { get; set; }
        public int GameplanDataTypePullId { get; set; }
        public string TargetDataType { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
    
        public virtual GameplanDataTypePull GameplanDataTypePull { get; set; }
        public virtual IntegrationInstance IntegrationInstance { get; set; }
    }
}

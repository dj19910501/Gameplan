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
    
    public partial class GameplanDataTypePull
    {
        public GameplanDataTypePull()
        {
            this.IntegrationInstanceDataTypeMappingPulls = new HashSet<IntegrationInstanceDataTypeMappingPull>();
        }
    
        public int GameplanDataTypePullId { get; set; }
        public int IntegrationTypeId { get; set; }
        public string ActualFieldName { get; set; }
        public string DisplayFieldName { get; set; }
        public string Type { get; set; }
        public bool IsDeleted { get; set; }
    
        public virtual IntegrationType IntegrationType { get; set; }
        public virtual ICollection<IntegrationInstanceDataTypeMappingPull> IntegrationInstanceDataTypeMappingPulls { get; set; }
    }
}

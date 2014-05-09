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
    
    public partial class IntegrationType
    {
        public IntegrationType()
        {
            this.IntegrationInstances = new HashSet<IntegrationInstance>();
            this.GameplanDataTypes = new HashSet<GameplanDataType>();
        }
    
        public int IntegrationTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public string APIVersion { get; set; }
        public string APIURL { get; set; }
    
        public virtual ICollection<IntegrationInstance> IntegrationInstances { get; set; }
        public virtual ICollection<GameplanDataType> GameplanDataTypes { get; set; }
    }
}

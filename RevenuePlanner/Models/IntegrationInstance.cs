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
    
    public partial class IntegrationInstance
    {
        public IntegrationInstance()
        {
            this.IntegrationInstance_Attribute = new HashSet<IntegrationInstance_Attribute>();
        }
    
        public int IntegrationInstanceId { get; set; }
        public int IntegrationTypeId { get; set; }
        public System.Guid ClientId { get; set; }
        public string Instance { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public Nullable<System.DateTime> LastSyncDate { get; set; }
        public string LastSyncStatus { get; set; }
        public bool IsImportActuals { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
    
        public virtual ICollection<IntegrationInstance_Attribute> IntegrationInstance_Attribute { get; set; }
        public virtual IntegrationType IntegrationType { get; set; }
        public virtual SyncFrequency SyncFrequency { get; set; }
    }
}

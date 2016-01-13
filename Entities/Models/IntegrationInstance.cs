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
            this.IntegrationInstanceDataTypeMappings = new HashSet<IntegrationInstanceDataTypeMapping>();
            this.IntegrationInstanceDataTypeMappingPulls = new HashSet<IntegrationInstanceDataTypeMappingPull>();
            this.IntegrationInstanceExternalServers = new HashSet<IntegrationInstanceExternalServer>();
            this.IntegrationInstanceLogs = new HashSet<IntegrationInstanceLog>();
            this.IntegrationInstancePlanEntityLogs = new HashSet<IntegrationInstancePlanEntityLog>();
            this.IntegrationInstanceSections = new HashSet<IntegrationInstanceSection>();
            this.Models = new HashSet<Model>();
            this.Models1 = new HashSet<Model>();
            this.Models2 = new HashSet<Model>();
            this.Models3 = new HashSet<Model>();
            this.IntegrationInstance_UnprocessData = new HashSet<IntegrationInstance_UnprocessData>();
            this.Models4 = new HashSet<Model>();
            this.IntegrationWorkFrontTemplates = new HashSet<IntegrationWorkFrontTemplate>();
            this.Models11 = new HashSet<Model>();
            this.IntegrationWorkFrontRequests = new HashSet<IntegrationWorkFrontRequest>();
            this.IntegrationWorkFrontRequestQueues = new HashSet<IntegrationWorkFrontRequestQueue>();
            this.IntegrationWorkFrontUsers = new HashSet<IntegrationWorkFrontUser>();
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
        public bool CustomNamingPermission { get; set; }
        public bool IsFirstPullCW { get; set; }
    
        public virtual ICollection<IntegrationInstance_Attribute> IntegrationInstance_Attribute { get; set; }
        public virtual IntegrationType IntegrationType { get; set; }
        public virtual ICollection<IntegrationInstanceDataTypeMapping> IntegrationInstanceDataTypeMappings { get; set; }
        public virtual ICollection<IntegrationInstanceDataTypeMappingPull> IntegrationInstanceDataTypeMappingPulls { get; set; }
        public virtual ICollection<IntegrationInstanceExternalServer> IntegrationInstanceExternalServers { get; set; }
        public virtual ICollection<IntegrationInstanceLog> IntegrationInstanceLogs { get; set; }
        public virtual ICollection<IntegrationInstancePlanEntityLog> IntegrationInstancePlanEntityLogs { get; set; }
        public virtual ICollection<IntegrationInstanceSection> IntegrationInstanceSections { get; set; }
        public virtual ICollection<Model> Models { get; set; }
        public virtual ICollection<Model> Models1 { get; set; }
        public virtual ICollection<Model> Models2 { get; set; }
        public virtual ICollection<Model> Models3 { get; set; }
        public virtual SyncFrequency SyncFrequency { get; set; }
        public virtual ICollection<IntegrationInstance_UnprocessData> IntegrationInstance_UnprocessData { get; set; }
        public virtual ICollection<Model> Models4 { get; set; }
        public virtual ICollection<IntegrationWorkFrontTemplate> IntegrationWorkFrontTemplates { get; set; }
        public virtual ICollection<Model> Models11 { get; set; }
        public virtual ICollection<IntegrationWorkFrontRequest> IntegrationWorkFrontRequests { get; set; }
        public virtual ICollection<IntegrationWorkFrontRequestQueue> IntegrationWorkFrontRequestQueues { get; set; }
        public virtual ICollection<IntegrationWorkFrontUser> IntegrationWorkFrontUsers { get; set; }
    }
}

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
    
    public partial class MarketoEntityValueMapping
    {
        public int ID { get; set; }
        public Nullable<int> EntityID { get; set; }
        public string EntityType { get; set; }
        public string MarketoCampaignFolderId { get; set; }
        public string ProgramType { get; set; }
        public string Channel { get; set; }
        public Nullable<int> IntegrationInstanceId { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public int LastModifiedBy { get; set; }
    
        public virtual IntegrationInstance IntegrationInstance { get; set; }
    }
}

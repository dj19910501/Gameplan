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
    
    public partial class Model
    {
        public Model()
        {
            this.LineItemTypes = new HashSet<LineItemType>();
            this.Model1 = new HashSet<Model>();
            this.Plans = new HashSet<Plan>();
            this.TacticTypes = new HashSet<TacticType>();
        }
    
        public int ModelId { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public int Year { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<bool> IsBenchmarked { get; set; }
        public Nullable<int> ParentModelId { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<int> IntegrationInstanceId { get; set; }
        public Nullable<int> IntegrationInstanceIdINQ { get; set; }
        public Nullable<int> IntegrationInstanceIdMQL { get; set; }
        public Nullable<int> IntegrationInstanceIdCW { get; set; }
        public double AverageDealSize { get; set; }
        public Nullable<int> IntegrationInstanceIdProjMgmt { get; set; }
        public Nullable<int> IntegrationInstanceEloquaId { get; set; }
        public Nullable<int> IntegrationInstanceMarketoID { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public int ClientId { get; set; }
    
        public virtual IntegrationInstance IntegrationInstance { get; set; }
        public virtual IntegrationInstance IntegrationInstance1 { get; set; }
        public virtual IntegrationInstance IntegrationInstance2 { get; set; }
        public virtual IntegrationInstance IntegrationInstance3 { get; set; }
        public virtual IntegrationInstance IntegrationInstance4 { get; set; }
        public virtual ICollection<LineItemType> LineItemTypes { get; set; }
        public virtual ICollection<Model> Model1 { get; set; }
        public virtual Model Model2 { get; set; }
        public virtual ICollection<Plan> Plans { get; set; }
        public virtual ICollection<TacticType> TacticTypes { get; set; }
        public virtual IntegrationInstance IntegrationInstance11 { get; set; }
        public virtual IntegrationInstance IntegrationInstance6 { get; set; }
    }
}

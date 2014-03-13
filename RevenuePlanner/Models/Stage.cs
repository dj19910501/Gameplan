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
    
    public partial class Stage
    {
        public Stage()
        {
            this.Model_Funnel_Stage = new HashSet<Model_Funnel_Stage>();
            this.TacticTypes = new HashSet<TacticType>();
        }
    
        public int StageId { get; set; }
        public string Title { get; set; }
        public System.Guid ClientId { get; set; }
        public string Description { get; set; }
        public Nullable<int> Level { get; set; }
        public string ColorCode { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public string Funnel { get; set; }
        public string Code { get; set; }
    
        public virtual ICollection<Model_Funnel_Stage> Model_Funnel_Stage { get; set; }
        public virtual ICollection<TacticType> TacticTypes { get; set; }
    }
}

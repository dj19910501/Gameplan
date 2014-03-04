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
    
    public partial class Model_Funnel
    {
        public Model_Funnel()
        {
            this.Model_Funnel_Stage = new HashSet<Model_Funnel_Stage>();
            this.ModelReviews = new HashSet<ModelReview>();
        }
    
        public int ModelFunnelId { get; set; }
        public int ModelId { get; set; }
        public int FunnelId { get; set; }
        public long ExpectedLeadCount { get; set; }
        public double AverageDealSize { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
    
        public virtual Funnel Funnel { get; set; }
        public virtual Model Model { get; set; }
        public virtual ICollection<Model_Funnel_Stage> Model_Funnel_Stage { get; set; }
        public virtual ICollection<ModelReview> ModelReviews { get; set; }
    }
}

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
    
    public partial class Model_Audience_Outbound
    {
        public int ModelAudienceOutboundId { get; set; }
        public int ModelId { get; set; }
        public Nullable<double> NormalErosion { get; set; }
        public Nullable<double> UnsubscribeRate { get; set; }
        public Nullable<int> NumberofTouches { get; set; }
        public Nullable<double> CTRDelivered { get; set; }
        public Nullable<double> RegistrationRate { get; set; }
        public string Quarter { get; set; }
        public Nullable<int> ListAcquisitions { get; set; }
        public Nullable<double> ListAcquisitionsNormalErosion { get; set; }
        public Nullable<double> ListAcquisitionsUnsubscribeRate { get; set; }
        public Nullable<double> ListAcquisitionsCTRDelivered { get; set; }
        public Nullable<double> Acquisition_CostperContact { get; set; }
        public Nullable<int> Acquisition_NumberofTouches { get; set; }
        public Nullable<double> Acquisition_RegistrationRate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<bool> IsBenchmarked { get; set; }
    
        public virtual Model Model { get; set; }
    }
}

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
    
    public partial class GetLinkedLineItemsForTransaction_Result
    {
        public int TacticId { get; set; }
        public string Title { get; set; }
        public double PlannedCost { get; set; }
        public Nullable<double> TotalLinkedCost { get; set; }
        public Nullable<double> TotalActual { get; set; }
    }
}

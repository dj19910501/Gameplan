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
    
    public partial class GetTacticLineItemList_Result
    {
        public int PlanLineItemId { get; set; }
        public int PlanTacticId { get; set; }
        public Nullable<int> LineItemTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public double Cost { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> LinkedLineItemId { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
    }
}
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
    
    public partial class Plan_Budget_Cost_Actual_Detail_Result
    {
        public string Id { get; set; }
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public string ParentActivityId { get; set; }
        public double MainBudgeted { get; set; }
        public int IsOwner { get; set; }
        public int CreatedBy { get; set; }
        public int IsAfterApproved { get; set; }
        public int IsEditable { get; set; }
        public double Cost { get; set; }
        public Nullable<double> Y1 { get; set; }
        public Nullable<double> Y2 { get; set; }
        public Nullable<double> Y3 { get; set; }
        public Nullable<double> Y4 { get; set; }
        public Nullable<double> Y5 { get; set; }
        public Nullable<double> Y6 { get; set; }
        public Nullable<double> Y7 { get; set; }
        public Nullable<double> Y8 { get; set; }
        public Nullable<double> Y9 { get; set; }
        public Nullable<double> Y10 { get; set; }
        public Nullable<double> Y11 { get; set; }
        public Nullable<double> Y12 { get; set; }
        public Nullable<double> CY1 { get; set; }
        public Nullable<double> CY2 { get; set; }
        public Nullable<double> CY3 { get; set; }
        public Nullable<double> CY4 { get; set; }
        public Nullable<double> CY5 { get; set; }
        public Nullable<double> CY6 { get; set; }
        public Nullable<double> CY7 { get; set; }
        public Nullable<double> CY8 { get; set; }
        public Nullable<double> CY9 { get; set; }
        public Nullable<double> CY10 { get; set; }
        public Nullable<double> CY11 { get; set; }
        public Nullable<double> CY12 { get; set; }
        public double TotalBudgetSum { get; set; }
        public double TotalCostSum { get; set; }
        public Nullable<int> LineItemTypeId { get; set; }
    }
}

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
    
    public partial class spGetPlanCalendarData_Result
    {
        public string id { get; set; }
        public string text { get; set; }
        public string machineName { get; set; }
        public string start_date { get; set; }
        public Nullable<System.DateTime> endDate { get; set; }
        public Nullable<double> duration { get; set; }
        public Nullable<double> progress { get; set; }
        public Nullable<bool> open { get; set; }
        public Nullable<bool> isSubmitted { get; set; }
        public Nullable<bool> isDeclined { get; set; }
        public Nullable<double> projectedStageValue { get; set; }
        public Nullable<double> mqls { get; set; }
        public Nullable<double> cost { get; set; }
        public Nullable<double> cws { get; set; }
        public string parent { get; set; }
        public string color { get; set; }
        public string colorcode { get; set; }
        public Nullable<int> PlanTacticId { get; set; }
        public Nullable<int> PlanProgramId { get; set; }
        public Nullable<int> PlanCampaignId { get; set; }
        public string Status { get; set; }
        public Nullable<int> TacticTypeId { get; set; }
        public string TacticType { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<bool> LinkTacticPermission { get; set; }
        public Nullable<int> LinkedTacticId { get; set; }
        public string LinkedPlanName { get; set; }
        public string type { get; set; }
        public string ROITacticType { get; set; }
        public string OwnerName { get; set; }
        public Nullable<int> IsAnchorTacticId { get; set; }
        public string CalendarHoneycombpackageIDs { get; set; }
        public Nullable<bool> Permission { get; set; }
        public Nullable<long> PlanId { get; set; }
    }
}

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
    
    public partial class Plan_Campaign_Program_Tactic
    {
        public Plan_Campaign_Program_Tactic()
        {
            this.IntegrationWorkFrontRequests = new HashSet<IntegrationWorkFrontRequest>();
            this.IntegrationWorkFrontTacticSettings = new HashSet<IntegrationWorkFrontTacticSetting>();
            this.Plan_Campaign_Program_Tactic_Actual = new HashSet<Plan_Campaign_Program_Tactic_Actual>();
            this.Plan_Campaign_Program_Tactic_Cost = new HashSet<Plan_Campaign_Program_Tactic_Cost>();
            this.Plan_Campaign_Program_Tactic_Budget = new HashSet<Plan_Campaign_Program_Tactic_Budget>();
            this.Plan_Campaign_Program_Tactic_Comment = new HashSet<Plan_Campaign_Program_Tactic_Comment>();
            this.Plan_Campaign_Program_Tactic_LineItem = new HashSet<Plan_Campaign_Program_Tactic_LineItem>();
            this.ROI_PackageDetail = new HashSet<ROI_PackageDetail>();
            this.ROI_PackageDetail1 = new HashSet<ROI_PackageDetail>();
            this.Tactic_Share = new HashSet<Tactic_Share>();
        }
    
        public int PlanTacticId { get; set; }
        public int PlanProgramId { get; set; }
        public int TacticTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public double Cost { get; set; }
        public double TacticBudget { get; set; }
        public string Status { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDeployedToIntegration { get; set; }
        public string IntegrationInstanceTacticId { get; set; }
        public Nullable<System.DateTime> LastSyncDate { get; set; }
        public Nullable<double> ProjectedStageValue { get; set; }
        public int StageId { get; set; }
        public string TacticCustomName { get; set; }
        public string IntegrationWorkFrontProjectID { get; set; }
        public string IntegrationInstanceEloquaId { get; set; }
        public Nullable<int> LinkedTacticId { get; set; }
        public Nullable<int> LinkedPlanId { get; set; }
        public Nullable<bool> IsSyncSalesForce { get; set; }
        public Nullable<bool> IsSyncEloqua { get; set; }
        public Nullable<bool> IsSyncWorkFront { get; set; }
        public Nullable<bool> IsSyncMarketo { get; set; }
        public string IntegrationInstanceMarketoID { get; set; }
        public int ModifiedBy { get; set; }
        public int CreatedBy { get; set; }
    
        public virtual ICollection<IntegrationWorkFrontRequest> IntegrationWorkFrontRequests { get; set; }
        public virtual ICollection<IntegrationWorkFrontTacticSetting> IntegrationWorkFrontTacticSettings { get; set; }
        public virtual Plan_Campaign_Program Plan_Campaign_Program { get; set; }
        public virtual ICollection<Plan_Campaign_Program_Tactic_Actual> Plan_Campaign_Program_Tactic_Actual { get; set; }
        public virtual ICollection<Plan_Campaign_Program_Tactic_Cost> Plan_Campaign_Program_Tactic_Cost { get; set; }
        public virtual ICollection<Plan_Campaign_Program_Tactic_Budget> Plan_Campaign_Program_Tactic_Budget { get; set; }
        public virtual ICollection<Plan_Campaign_Program_Tactic_Comment> Plan_Campaign_Program_Tactic_Comment { get; set; }
        public virtual ICollection<Plan_Campaign_Program_Tactic_LineItem> Plan_Campaign_Program_Tactic_LineItem { get; set; }
        public virtual Plan_Campaign_Program_Tactic Plan_Campaign_Program_Tactic1 { get; set; }
        public virtual Plan_Campaign_Program_Tactic Plan_Campaign_Program_Tactic2 { get; set; }
        public virtual Stage Stage { get; set; }
        public virtual TacticType TacticType { get; set; }
        public virtual ICollection<ROI_PackageDetail> ROI_PackageDetail { get; set; }
        public virtual ICollection<ROI_PackageDetail> ROI_PackageDetail1 { get; set; }
        public virtual ICollection<Tactic_Share> Tactic_Share { get; set; }
    }
}

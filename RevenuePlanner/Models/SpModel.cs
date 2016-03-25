﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class SpModel
    {

    }
    public class PlanCampProgTac
    {
        public List<Custom_Plan> Plan { get; set; }
        public List<Custom_Plan_Campaign> Plan_Campaign { get; set; }
        public List<Custom_Plan_Campaign_Program> Plan_Campaign_Program { get; set; }
        public List<Plan_Campaign_Program_Tactic> Plan_Campaign_Program_Tactic { get; set; }
    }

    public class Custom_Plan
    {
        public int PlanId { get; set; }
        public int ModelId { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string Year { get; set; }
        public string Description { get; set; }
        public double Budget { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public string GoalType { get; set; }
        public double GoalValue { get; set; }
        public string AllocatedBy { get; set; }
        public string EloquaFolderPath { get; set; }
        public Nullable<System.DateTime> DependencyDate { get; set; }
    }
    public class Custom_Plan_Campaign
    {
        public int PlanCampaignId { get; set; }
        public int PlanId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public string Status { get; set; }
        public bool IsDeployedToIntegration { get; set; }
        public string IntegrationInstanceCampaignId { get; set; }
        public Nullable<System.DateTime> LastSyncDate { get; set; }
        public double CampaignBudget { get; set; }
        public string Abbreviation { get; set; }
    }

    public class Custom_Plan_Campaign_Program
    {
        public int PlanProgramId { get; set; }
        public int PlanCampaignId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public string Status { get; set; }
        public bool IsDeployedToIntegration { get; set; }
        public string IntegrationInstanceProgramId { get; set; }
        public Nullable<System.DateTime> LastSyncDate { get; set; }
        public double ProgramBudget { get; set; }
        public string Abbreviation { get; set; }
        // Custom Columns
        public int PlanId { get; set; }
    }

    public class Custom_Plan_Campaign_Program_Tactic
    {
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
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
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

        // Custom Columns
        public int PlanId { get; set; }
        public int PlanCampaignId { get; set; }
        public string TacticTypeTtile { get; set; }
        public string ColorCode { get; set; }
        public string PlanYear { get; set; }
        public int ModelId { get; set; }
    }
}

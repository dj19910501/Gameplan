using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RevenuePlanner.Models
{
    public class HomePlanModel
    {
        public HomePlanModelHeader objplanhomemodelheader { get; set; }
        public HomePlan objHomePlan { get; set; }

        //Start Maninder Singh Wadhva : 11/26/2013 - planId.
        public int PlanId;
        public List<int> lstPlanId;
        //End Maninder Singh Wadhva : 11/26/2013 - planId.

        //Start Maninder Singh Wadhva : 11/27/2013 - Director.
        //    public bool IsDirector;
        //End Maninder Singh Wadhva : 11/27/2013 - Director.

        public List<BDSService.User> objIndividuals { get; set; }

        //  public List<SelectListItem> plans { get; set; }

        //Start Maninder Singh Wadhva : 12/03/2013 - plan title.
        public string PlanTitle;
        //End Maninder Singh Wadhva : 12/03/2013 - plan title.

        //// Modified By Maninder Singh Wadhva to Address PL#203
        //public List<string> CollaboratorId;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:g}")]
        public DateTime LastUpdatedDate { get; set; }
        public List<CustomFieldsForFilter> lstCustomFields { get; set; }
        public List<CustomFieldsForFilter> lstCustomFieldOptions { get; set; }
        public List<OwnerModel> lstOwner { get; set; }
        public List<TacticTypeModel> lstTacticType { get; set; }
        public List<PlanListModel> lstPlan { get; set; }
        // Add By Nishant Sheth 
        // #1765 - to set the item selected plan year
        public string SelectedPlanYear { get; set; }
    }
    public class PlanListModel
    {
        public int PlanId { get; set; }
        public string Title { get; set; }
        public string Checked { get; set; }
        public string Year { get; set; }
    }
    public class TacticTypeModel
    {
        public int TacticTypeId { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
		// Added by Arpita Soni for Ticket #2354 on 07/19/2016
        public string AssetType { get; set; }
    }
    public class OwnerModel
    {
        public string OwnerId { get; set; }
        public string Title { get; set; }
    }

    public class HomePlan
    {
        //public bool IsDirector { get; set; }
        //public bool IsClientAdmin { get; set; }
        public bool IsManager { get; set; } // Modified by Dharmraj, #538
        public List<SelectListItem> plans { get; set; }
    }
    public class HomePlanModelHeader
    {
        public double MQLs { get; set; }
        public double Budget { get; set; }
        public int TacticCount { get; set; }
        public string mqlLabel { get; set; }
        public string costLabel { get; set; }

        /// Added By: Maninder Singh Wadhva
        /// Addressed PL Ticket: 37,38,47,49
        public double? PercentageMQLImproved { get; set; }

        // public List<string> UpcomingActivity { get; set; }
        public List<SelectListItem> UpcomingActivity { get; set; }
        public List<SelectListItem> plans { get; set; }
    }

    public class InspectModel
    {
        public int PlanTacticId { get; set; }

        public string TacticTitle { get; set; }

        public string TacticTypeTitle { get; set; }

        public string CampaignTitle { get; set; }

        public string ProgramTitle { get; set; }

        public string Status { get; set; }

        public int TacticTypeId { get; set; }

        public string ColorCode { get; set; }

        public string Description { get; set; }

        public int PlanCampaignId { get; set; }

        public int PlanProgramId { get; set; }

        public string Owner { get; set; }

        public Guid OwnerId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        //public long? INQs { get; set; }

        public long? INQsActual { get; set; }

        public double? MQLs { get; set; }

        public double? MQLsActual { get; set; }

        public double? CWs { get; set; }

        public double? CWsActual { get; set; }

        public double? Revenues { get; set; }

        public double? RevenuesActual { get; set; }

        public double? Cost { get; set; }

        public double? CostActual { get; set; }

        public double? ROI { get; set; }

        public double? ROIActual { get; set; }

        public string IsIntegrationInstanceExist { get; set; }

        public bool IsDeployedToIntegration { get; set; }

        public DateTime? LastSyncDate { get; set; }

        public int? StageId { get; set; }

        public string StageTitle { get; set; }

        public int? StageLevel { get; set; } 

        public double? ProjectedStageValue { get; set; }

        public double? ProjectedStageValueActual { get; set; }

        public int ImprovementPlanTacticId { get; set; }
        public int ImprovementPlanProgramId { get; set; }
        public int ImprovementTacticTypeId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Title { get; set; }
        public int PlanId { get; set; }
        public int ModelId { get; set; }
        public string ModelTitle { get; set; }
        public string GoalType { get; set; }
        public string GoalValue { get; set; }
        public double Budget { get; set; }
        public string AllocatedBy { get; set; }
        public string TacticCustomName { get; set; }

        public bool IsPlanEditable { get; set; }
        public bool IsPlanCreateAll { get; set; }
        public string InspectMode { get; set; }
        public string InspectPopup { get; set; }
        public string RedirectType { get; set; }

        public int PlanLineitemId { get; set; }

        //Add integration information
        public string WorkFrontTemplate { get; set; }
        public int? WorkFrontRequestQueueId { get; set; }
        public int? WorkFrontRequestAssignee { get; set; }
        public string WorkFrontTacticApprovalBehavior { get; set; }
        public Nullable<int> LinkedTacticId { get; set; } //Added by Rahul Shah on 12/04/2016 for PL #2038
        public string CampaignfolderValue{get;set;}
        public string programType{get;set;}
        public string Channel{get;set;}
        public string ROIType { get; set; }
    }

    public class InspectReviewModel
    {
        public int PlanTacticId { get; set; }

        public int PlanCampaignId { get; set; } //PlanCampaignId property added for InspectReview section of Campaign Inspect Popup

        public int PlanProgramId { get; set; } //PlanProgramId property added for InspectReview section of Program Inspect Popup

        public string Comment { get; set; }

        public DateTime CommentDate { get; set; }

        public string CommentedBy { get; set; }

        public Guid CreatedBy { get; set; }

    }

    public class InspectActual
    {
        public int PlanTacticId { get; set; }

        public string StageTitle { get; set; }

        public string Period { get; set; }

        public double ActualValue { get; set; }

        public long TotalProjectedStageValueActual { get; set; }

        public long TotalMQLActual { get; set; }

        public long TotalCWActual { get; set; }

        public double TotalRevenueActual { get; set; }

        public double TotalCostActual { get; set; }

        public double ROI { get; set; }

        public double ROIActual { get; set; }

		public bool IsActual { get; set; }

        public int StageId { get; set; }

        public int PlanLineItemId { get; set; }
		
    }

    public class ActivityChart
    {
        public string NoOfActivity { get; set; }
        public string Month { get; set; }
        public string Color { get; set; }
    }

    public class ImprovementTaskDetail
    {
        public string MainParentId { get; set; }
        public DateTime MinStartDate { get; set; }
        public int ImprovePlanId { get; set; }
        public Plan_Improvement_Campaign_Program_Tactic ImprovementTactic { get; set; }
        public System.Guid CreatedBy { get; set; } 
    }

    public class CustomFields
    {
        public string cfId { get; set; }
        public string Title { get; set; }
        public string ColorCode { get; set; }
    }

    public class TacticTaskList
    {
        public Plan_Campaign_Program_Tactic Tactic { get; set; }
        public Plan_Tactic PlanTactic { get; set; }
        public string CustomFieldType { get; set; }
        public string CustomFieldId { get; set; }
        public string CustomFieldTitle { get; set; }
        public string TaskId { get; set; }
        public string ColorCode { get; set; }
        public string StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Duration { get; set; }
        public double PlanProgrss { get; set; } 
        public double CampaignProgress { get; set; }
        public double ProgramProgress { get; set; }
        public List<int> lstCustomEntityId { get; set; }
        public System.Guid CreatedBy { get; set; } 
        public DateTime PlanStartDate { get; set; }
        public DateTime PlanEndDate { get; set; }
        public bool LinkTacticPermission { get; set; }
        public int? LinkedTacticId { get; set; }
        public Custom_Plan_Campaign_Program_Tactic CustomTactic { get; set; }
    }
    // Add By Nishant Sheth
    // Desc :: Store Progess details for Plan/Campaign/Program ticket #1798
    public class ProgressList
    {
        public int EntityId { get; set; }
        public double Progress { get; set; }
    }

    // Add By Nishant Sheth
    // Desc :: Store details for plan min date ticket #1798
    public class StartMinDatePlan
    {
        public int EntityId { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
    }
    // Add By Nishant Sheth
    // Desc :: Store details for plan min date and duration ticket #1798
    public class StartMin_Duration_Plan
    {
        public string ParentId { get; set; }
        public string PlanId { get; set; }
        public string MinStartDate { get; set; }
        public double Duration { get; set; }
    }
    // Add By Nishant Sheth
    // Desc :: Store details for plan min date and end date ticket #1798
    public class StartMin_EndMax_Plan
    {
        public string ParentId { get; set; }
        public string PlanId { get; set; }
        public DateTime MinStartDate { get; set; }
        public DateTime MaxEndDate { get; set; }
    }
    // Add By Nishant Sheth
    // Desc :: Store details for plan task data #1798
    public class TaskDataPlan
    {
        public string id { get; set; }
        public string text { get; set; }
        public string start_date { get; set; }
        public double duration { get; set; }
        public double progress { get; set; }
        public bool open { get; set; }
        public string parent { get; set; }
        public string color { get; set; }
        public int planid { get; set; }
        public Guid CreatedBy { get; set; }
        public string Status{ get; set; }
    }
    public class Plan_Tactic
    {
        public Plan_Campaign_Program_Tactic objPlanTactic { get; set; }
        public int PlanCampaignId { get; set; }
        public int PlanId { get; set; }
        public TacticType TacticType { get; set; }
        public Plan_Campaign_Program objPlanTacticProgram { get; set; }
        public Plan_Campaign objPlanTacticCampaign { get; set; }
        public Plan objPlanTacticCampaignPlan { get; set; }
        public System.Guid CreatedBy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class CustomFieldsForFilter
    {
        public int CustomFieldId { get; set; }
        public string Title { get; set; }
        public int? CustomFieldOptionId { get; set; }
    }
    public partial class   lastSeen
    {
        public int Id { get; set; }
        public string ViewName { get; set; }
        public string FilterName { get; set; }
        public string FilterValues { get; set; }
        public Nullable<System.Guid> Userid { get; set; }
        public System.DateTime LastModifiedDate { get; set; }
    }
	public class PlanMainDHTMLXGrid
    {
        public List<PlanDHTMLXGridDataModel> rows { get; set; }
        public List<PlanHead> head { get; set; }
    }

    public class PlanDHTMLXGridDataModel
    {
        public string id { get; set; }
        public string open { get; set; }
        public string bgColor { get; set; }
        public List<Plandataobj> data { get; set; }
        public Planuserdatagrid userdata { get; set; }
        public List<PlanDHTMLXGridDataModel> rows { get; set; }
    }

    // Add By Nishant Sheth
    // class for get grid default/common data
    public class GridDefaultModel
    {
        public string UniqueId { get; set; }
        public Int64 EntityId { get; set; }
        public string EntityTitle { get; set; }
        public Nullable<Int64> ParentEntityId { get; set; }
        public string ParentUniqueId { get; set; }
        public string EntityType { get; set; }
        public string ColorCode { get; set; }
        public string Status { get; set; }
        public Nullable<DateTime> StartDate { get; set; }
        public Nullable<DateTime> EndDate { get; set; }
        public Guid CreatedBy { get; set; }
        public string AltId { get; set; }
        public string TaskId { get; set; }
        public string ParentTaskId { get; set; }
        public Int64 PlanId { get; set; }
        public Nullable<Int64> ModelId { get; set; }
        public string AssetType { get; set; }
        public string TacticType { get; set; }
        public Nullable<Int32> TacticTypeId { get; set; }
        public Nullable<Int32> LineItemTypeId { get; set; }
        public string LineItemType { get; set; }
        public Nullable<double> PlannedCost { get; set; }
        public Nullable<double> ProjectedStageValue { get; set; }
        public string ProjectedStage { get; set; }
        public Nullable<Int64> MQL { get; set; }
        public Nullable<decimal> Revenue { get; set; }
        public string MachineName { get; set; }
        public Nullable<int> LinkedPlanId { get; set; }
        public Nullable<int> LinkedTacticId { get; set; }
        public string LinkedPlanName { get; set; }
        public Nullable<int> AnchorTacticID { get; set; }
        public string PackageTacticIds { get; set; }
    }

    // Add By Nishant Sheth
    // Get list of custom fields
    public class GridCustomFields
    {
        public int CustomFieldId { get; set; }
        public string CustomFieldName { get; set; }
        public int CustomFieldTypeId { get; set; }
        public bool IsRequired { get; set; }
        public string EntityType { get; set; }
        public string AbbreviationForMulti { get; set; }
    }

    // Add By Nishant Sheth
    // Get list of Entity custom field values
    public class GridCustomFieldEntityValues
    {
        public Int64 EntityId { get; set; }
        public string EntityType { get; set; }
        public Nullable<int> CustomFieldId { get; set; }
        public Nullable<int> CustomFieldEntityId { get; set; }
        public string Value { get; set; }
    }

    // Add By Nishant Sheth
    // Combine list of custom field and it's entities values
    public class GridCustomColumnData
    {
        public List<GridCustomFields> CustomFields { get; set; }
        public List<GridCustomFieldEntityValues> CustomFieldValues { get; set; }
    }

    public class Planuserdatagrid
    {
        public string psdate { get; set; }
        public string pedate { get; set; }
        public string tsdate { get; set; }
        public string tedate { get; set; }
        public string stage { get; set; }
        public string tactictype { get; set; }
        public string IsOther { get; set; }
    }

    public class Plandataobj
    {
        public string value { get; set; }
        public string locked { get; set; }
        public string style { get; set; }
        public string actval { get; set; }
        public string type { get; set; }
    }

    public class PlanHead
    {
        public List<PlanOptions> options { get; set; }
        public string value { get; set; }
        public int width { get; set; }
        public string align { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public string sort { get; set; }
    }
    public class PlanOptions
    {
        public string id { get; set; }
        public string value { get; set; }
    }	  
    // Add By Nishant Sheth 
    // #1765 - to add list of monthly period.
    public class listMonthDynamic
    {
        public int Id { get; set; }
        public List<string> listMonthly { get; set; }
    }

    #region "Move Entity from one plan to other"
    // Added by Viral Kadiya : PL ticket #1748
    public class PlanTactic_TacticTypeMapping
    {
        public int PlanTacticId { get; set; }
        public int TacticTypeId { get; set; }
        public int? TargetStageId { get; set; }
    }
    public class CopyEntiyBetweenPlanModel
    {
        public List<DhtmlxGridRowDataModel> rows { get; set; }
        public string HeaderTitle { get; set; }
        public string srcSectionType { get; set; }
        public string srcEntityId { get; set; }
        public string srcPlanId { get; set; }
        public string destPlanId { get; set; }
        public string destEntityId { get; set; }
    }

    public class ParentChildEntityMapping
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string RowId { get; set; }
    } 
    #endregion
    public class Preset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool? IsDefaultPreset { get; set; }
    }

    public class GoalValueModel
    {
        public string Title { get; set; }
        public double Value { get; set; }

        public string StageCode { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using RevenuePlanner.Helpers;

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
        public int OwnerId { get; set; }
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

        public double? PercentageMQLImproved { get; set; }

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

        public int OwnerId { get; set; }

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
        public string CampaignfolderValue { get; set; }
        public string programType { get; set; }
        public string Channel { get; set; }
        public string ROIType { get; set; }

        public string Year { get; set; }
        public string AverageDealSize { get; set; }
        public double TotalAllocatedCampaignBudget { get; set; }
    }

    public class InspectReviewModel
    {
        public int PlanTacticId { get; set; }

        public int PlanCampaignId { get; set; } //PlanCampaignId property added for InspectReview section of Campaign Inspect Popup

        public int PlanProgramId { get; set; } //PlanProgramId property added for InspectReview section of Program Inspect Popup

        public string Comment { get; set; }

        public DateTime CommentDate { get; set; }

        public string CommentedBy { get; set; }

        public int CreatedBy { get; set; }

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
        public int CreatedBy { get; set; }
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
        public int CreatedBy { get; set; }
        public DateTime PlanStartDate { get; set; }
        public DateTime PlanEndDate { get; set; }
        public bool LinkTacticPermission { get; set; }
        public int? LinkedTacticId { get; set; }
        public Custom_Plan_Campaign_Program_Tactic CustomTactic { get; set; }
    }

    // Add By Nishant Sheth
    // Desc :: Get the Revenue / MQL values 
    public class ReveneueMqlData
    {
        public decimal Value { get; set; }
        public string ParentUniqueId { get; set; }
        public string UniqueId { get; set; }
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
        public int CreatedBy { get; set; }
        public string Status { get; set; }
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
        public int CreatedBy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class CustomFieldsForFilter
    {
        public int CustomFieldId { get; set; }
        public string Title { get; set; }
        public int? CustomFieldOptionId { get; set; }
    }
    public partial class lastSeen
    {
        public int Id { get; set; }
        public string ViewName { get; set; }
        public string FilterName { get; set; }
        public string FilterValues { get; set; }
        public int Userid { get; set; }
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
        public List<Plandataobj> data { get; set; }
        public Planuserdatagrid userdata { get; set; }
        public List<PlanDHTMLXGridDataModel> rows { get; set; }
    }

    public class PlanMainDHTMLXGridHomeGrid
    {
        public List<PlanDHTMLXGridDataModelHomeGrid> rows { get; set; }
        public List<PlanHead> head { get; set; }
    }

    public class PlanGridFilters
    {
        public string PlanIds { get; set; }
        public string OwnerIds { get; set; }
        public string StatusIds { get; set; }
        public string TacticTypeIds { get; set; }
        public string CustomFieldIds { get; set; }
    }

    public class PlanDHTMLXGridDataModelHomeGrid
    {
        public string id { get; set; }
        public string open { get; set; }
        public List<PlanGridDataobj> data { get; set; }
        public Planuserdatagrid userdata { get; set; }
        public List<PlanDHTMLXGridDataModelHomeGrid> rows { get; set; }
    }

    public class PlanGridDataobj
    {
        public string value { get; set; }
        public string lo { get; set; }
        public string style { get; set; }
        public string type { get; set; }
    }


    // Add By Nishant Sheth
    // Class for get dynamic column values of grid data
    public class PlanGridColumnData
    {
        public Int64? EntityId { get; set; }
        public Int32? Owner { get; set; }
        public string AltId { get; set; }
        public string ColorCode { get; set; }
        public string TaskId { get; set; }
        public string ParentTaskId { get; set; }
        public string UniqueId { get; set; }
        public string ParentUniqueId { get; set; }

        public string OwnerName { get; set; }

        public Enums.EntityType EntityType { get; set; }
        private Int64? _ParentEntityId;
        public Nullable<Int64> ParentEntityId
        {
            get
            {
                return this._ParentEntityId;
            }
            set
            {
                if (value == null)
                {
                    this._ParentEntityId = 0;
                }
                else
                {
                    this._ParentEntityId = value;
                }
            }
        }
        public string AssetType { get; set; }
        public string TacticType { get; set; }
        public Nullable<DateTime> StartDate { get; set; }
        public Nullable<DateTime> EndDate { get; set; }
        public string Status { get; set; }
        public string EntityTitle { get; set; }
        private Int32? _LineItemTypeId;
        public Nullable<Int32> LineItemTypeId
        {
            get
            {
                return this._LineItemTypeId;
            }
            set
            {
                if (value == null)
                {
                    this._LineItemTypeId = 0;
                }
                else
                {
                    this._LineItemTypeId = value;
                }
            }
        }
        public string LineItemType { get; set; }
        private int? _AnchorTacticID;
        public Nullable<int> AnchorTacticID
        {
            get
            {
                return this._AnchorTacticID;
            }
            set
            {
                if (value == null)
                {
                    this._AnchorTacticID = 0;
                }
                else
                {
                    this._AnchorTacticID = value;
                }
            }
        }
        //integrationids
        public string Eloquaid { get; set; }
        public string Marketoid { get; set; }
        public string WorkFrontid { get; set; }
        public string Salesforceid { get; set; }
        //end
        //Permission variables
        public bool IsCreatePermission { get; set; }
        public bool IsRowPermission { get; set; }
        public bool IsExtendTactic { get; set; }
        public int? LinkedTacticId { get; set; }
        public List<PlandataobjColumn> lstdata { get; set; }
        //public Plandataobj objdata { get; set; }
    }

    // Add By Nishant Sheth
    // class for get grid default/common data
    public class GridDefaultModel
    {
        public string UniqueId { get; set; }
        public Int64? EntityId { get; set; }
        public string EntityTitle { get; set; }
        public Nullable<Int64> ParentEntityId { get; set; }
        public string ParentUniqueId { get; set; }
        public Enums.EntityType EntityType { get; set; }
        public string ColorCode { get; set; }
        public string Status { get; set; }
        public Nullable<DateTime> StartDate { get; set; }
        public Nullable<DateTime> EndDate { get; set; }
        public Int32? Owner { get; set; }
        public string AltId { get; set; }
        public string TaskId { get; set; }
        public string ParentTaskId { get; set; }
        public Int64? PlanId { get; set; }
        public Nullable<Int64> ModelId { get; set; }
        public string AssetType { get; set; }
        public string TacticType { get; set; }
        public Nullable<Int32> TacticTypeId { get; set; }
        public Nullable<Int32> LineItemTypeId { get; set; }
        public string LineItemType { get; set; }
        public Nullable<double> PlannedCost { get; set; }
        public Nullable<double> ProjectedStageValue { get; set; }
        public string TargetStageGoal { get; set; }
        public string ProjectedStage { get; set; }
        public Nullable<Int64> MQL { get; set; }
        
        public Nullable<decimal> Revenue { get; set; }
        
        public string MachineName { get; set; }
        public Nullable<int> LinkedPlanId { get; set; }
        public Nullable<int> LinkedTacticId { get; set; }
        public string LinkedPlanName { get; set; }
        public Nullable<int> AnchorTacticID { get; set; }
        //integrationids
        public string Eloquaid { get; set; }
        public string Marketoid { get; set; }
        public string WorkFrontid { get; set; }
        public string Salesforceid { get; set; }
        //end
        public string PackageTacticIds { get; set; }
        public string PlanYear { get; set; }

        public string OwnerName { get; set; }

        //Permission variables
        public bool IsCreatePermission { get; set; }
        public bool IsRowPermission { get; set; }
        public List<PlandataobjColumn> lstdata { get; set; }
        public bool IsExtendTactic { get; set; }
    }

    // Add By Nishant Sheth
    // Class for grid row permission for edit or not
    public class EntityPermissionRowWise
    {
        public Enums.EntityType EntityType { get; set; }
        public string EntityId { get; set; }
        public bool IsRowPermission { get; set; }
        public string LineItemType { get; set; }
    }
    // Add By Nishant Sheth
    // Class For User default Permission
    public class EntityPermission
    {
        public bool PlanCreate { get; set; }
        public bool PlanEditSubordinates { get; set; }
        public bool PlanEditAll { get; set; }
    }

    // Add By Nishant Sheth
    // Get custom fields values for entities in pivoted 
    public class CustomfieldPivotData
    {
        public string UniqueId { get; set; }
        public Enums.EntityType EntityType { get; set; }
        public List<PlandataobjColumn> CustomFieldData { get; set; }
    }

    // Add By Nishant Sheth
    // Get list of custom fields
    public class GridCustomFields
    {
        public int CustomFieldId { get; set; }
        public string CustomFieldName { get; set; }
        public int CustomFieldTypeId { get; set; }
        public bool IsRequired { get; set; }
        public Enums.EntityType EntityType { get; set; }
        public string AbbreviationForMulti { get; set; }
        public string CustomFieldType { get; set; }
        public string CustomUniqueId { get; set; }
    }

    // Add By Nishant Sheth
    // Below class used for respective entity custom fields cells are editable or not on home grid
    public class EmptyCustomFieldsValuesEntity
    {
        public List<PlandataobjColumn> PlanEmptyCustomFields { get; set; }
        public List<PlandataobjColumn> CampaignEmptyCustomFields { get; set; }
        public List<PlandataobjColumn> ProgramEmptyCustomFields { get; set; }
        public List<PlandataobjColumn> TacticEmptyCustomFields { get; set; }
        public List<PlandataobjColumn> LineitemEmptyCustomFields { get; set; }
        public List<PlandataobjColumn> ViewOnlyCustomFields { get; set; }
    }
    // Add By Nishant Sheth
    // Get list of view only and none permission custom field option
    public class UserRestrictedValues
    {
        public int CustomFieldId { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
    }

    // Add By Nishant Sheth
    // Get list of Entity custom field values
    public class GridCustomFieldEntityValues
    {
        private Int64? _EntityId;
        public Nullable<Int64> EntityId
        {
            get
            {
                return this._EntityId;
            }
            set
            {
                if (value == null)
                {
                    this._EntityId = 0;
                }
                else
                {
                    this._EntityId = value;
                }
            }
        }
        public Enums.EntityType EntityType { get; set; }
        private int? _CustomFieldId;
        public Nullable<int> CustomFieldId
        {
            get
            {
                return this._CustomFieldId;
            }
            set
            {
                if (value == null)
                {
                    this._CustomFieldId = 0;
                }
                else
                {
                    this._CustomFieldId = value;
                }
            }
        }
        public string Value { get; set; }
        public string UniqueId { get; set; }
        public string Text { get; set; }
        public string RestrictedText { get; set; }
        public string RestrictedValue { get; set; }
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
    // Class use for plan grid data with column name // use for display order wise column on plan grid
    public class PlandataobjColumn
    {
        public string value { get; set; }
        public string locked { get; set; }
        public string style { get; set; }
        public string actval { get; set; }
        public string type { get; set; }
        public string column { get; set; }
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
        public int id { get; set; } //generid id field including user ID but all are integers - zz
        public string value { get; set; }
    }
    public class PlanOptionsTacticType : PlanOptions
    {
        public int PlanId { get; set; }
        public string Type { get; set; }
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

    public class calendarDataModel
    {
        public string id { get; set; }
        public string text { get; set; }
        public string machineName { get; set; }
        public string start_date { get; set; }
        public DateTime? endDate { get; set; }
        public double duration { get; set; }
        public double progress { get; set; }
        public bool? open { get; set; }
        public bool? isSubmitted { get; set; }
        public bool? isDeclined { get; set; }
        public double? projectedStageValue { get; set; }
        public double? mqls { get; set; }
        public double? cost { get; set; }
        public double? cws { get; set; }
        public string parent { get; set; }
        public string color { get; set; }
        public string colorcode { get; set; }
        public int? PlanTacticId { get; set; }
        public int? PlanProgramId { get; set; }
        public int? PlanCampaignId { get; set; }
        public string Status { get; set; }
        public int? TacticTypeId { get; set; }
        public string TacticType { get; set; }
        public int? CreatedBy { get; set; }
        public bool? LinkTacticPermission { get; set; }
        public int? LinkedTacticId { get; set; }
        public string LinkedPlanName { get; set; }
        public string type { get; set; }
        public string ROITacticType { get; set; }
        public string OwnerName { get; set; }
        public int? IsAnchorTacticId { get; set; }
        public string CalendarHoneycombpackageIDs { get; set; }
        public bool? Permission { get; set; }
        public long? PlanId { get; set; }
        public int? PYear { get; set; }
        public bool? IsRowPermission { get; set; }
    }

    public class SearchParentDetail
    {
        public string ParentTaskId { get; set; }
        public string TaskId { get; set; }

    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Models
{
    public class PlanModel
    {
        [Key]
        public int PlanId { get; set; }

        [Required]
        public string Title { get; set; }

        // Commented by Sohel Pathan on 15/07/2014 for PL ticket #566
        //[Required]    
        //public string MQls { get; set; }

        [Required]
        public double Budget { get; set; }

        // added by Nirav Shah 12/20/2013
        [Required]
        public string Year { get; set; }

        public int ModelId { get; set; }
        public string ModelTitle { get; set; }

        //Added By Bhavesh
        public double MQLDisplay { get; set; }

        //Added By Kunal
        public string Version { get; set; }

        public bool IsDirector { get; set; }

        /* Start - Added by Sohel Pathan on 15/07/2014 for PL ticket #566 */  
        public string GoalType { get; set; }
        public string GoalValue { get; set; }
        public string AllocatedBy { get; set; }
        public string AverageDealSize { get; set; }
        public string GoalTypeDisplay { get; set; }
        /* End - Added by Sohel Pathan on 15/07/2014 for PL ticket #566 */

        /* Added by Sohel Pathan on 22/07/2014 for PL ticket #597 */  
        public double TotalAllocatedCampaignBudget { get; set; }

        /* Added by Sohel Pathan on 04/08/2014 for PL ticket #623 */
        public string Description { get; set; }
    }

    public class Plan_CampaignModel
    {
        public int PlanId { get; set; }
        public int PlanCampaignId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int VerticalId { get; set; }
        public int AudienceId { get; set; }
        public Guid GeographyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //public long? INQs { get; set; }
        public double? MQLs { get; set; }
        public double? Cost { get; set; }
        public DateTime? PStartDate { get; set; }
        public DateTime? PEndDate { get; set; }
        public DateTime? TStartDate { get; set; }
        public DateTime? TEndDate { get; set; }

        public bool IsDeployedToIntegration { get; set; }
        public double CampaignBudget { get; set; }
        public string AllocatedBy { get; set; }
        public double Revenue { get; set; }
        //Added by Mitesh Vaishnav for PL ticket #718
        public MvcHtmlString CustomFieldHtmlContent { get; set; }
    }

    public class Plan_Campaign_ProgramModel
    {
        public int PlanProgramId { get; set; }
        public int PlanCampaignId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int VerticalId { get; set; }
        public int AudienceId { get; set; }
        public Guid GeographyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //public long? INQs { get; set; }
        public double? MQLs { get; set; }
        public double? Cost { get; set; }

        public DateTime CStartDate { get; set; }
        public DateTime CEndDate { get; set; }
        public DateTime? TStartDate { get; set; }
        public DateTime? TEndDate { get; set; }

        public bool IsDeployedToIntegration { get; set; }

        //Start added by Kalpesh  #608: Budget allocation for Program
        public double ProgramBudget { get; set; }
        public string AllocatedBy { get; set; }
        public double Revenue { get; set; }
    }

    public class Plan_Campaign_Program_TacticModel
    {
        public int PlanTacticId { get; set; }
        public int PlanProgramId { get; set; }
        public int TacticTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int VerticalId { get; set; }
        public int AudienceId { get; set; }
        public Guid GeographyId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //public long INQs { get; set; }
        public double MQLs { get; set; }
        public double Cost { get; set; }

        public DateTime PStartDate { get; set; }
        public DateTime PEndDate { get; set; }
        public DateTime CStartDate { get; set; }
        public DateTime CEndDate { get; set; }

        public bool IsDeployedToIntegration { get; set; }

        public int StageId { get; set; }
        public string StageTitle { get; set; }
        public double ProjectedStageValue { get; set; }


        //Start by Kalpesh Sharma #605: Cost allocation for Tactic
        public double TacticCost { get; set; }
        public string AllocatedBy { get; set; }
        public double Revenue { get; set; }
        //Added by Mitesh Vaishnav for PL ticket #720
        public MvcHtmlString CustomFieldHtmlContent { get; set; }
    }

    public class Inspect_Popup_Plan_Campaign_Program_TacticModel
    {
        public int PlanProgramId { get; set; }
        public int PlanCampaignId { get; set; }
        public int PlanTacticId { get; set; }

        public string TacticTitle { get; set; }
        public string ProgramTitle { get; set; }
        public string CampaignTitle { get; set; }

        public int TacticTypeId { get; set; }
        
        public string Description { get; set; }

        public string Owner { get; set; }
        public Guid OwnerId { get; set; }

        public int VerticalId { get; set; }
        public int AudienceId { get; set; }
        public Guid GeographyId { get; set; }

        public Guid BusinessUnitId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double MQLs { get; set; }
        public double Cost { get; set; }

        public DateTime PStartDate { get; set; }
        public DateTime PEndDate { get; set; }
        public DateTime CStartDate { get; set; }
        public DateTime CEndDate { get; set; }

        public bool IsDeployedToIntegration { get; set; }

        public int StageId { get; set; }
        public string StageTitle { get; set; }
        public double ProjectedStageValue { get; set; }


        public double TacticCost { get; set; }
        public string AllocatedBy { get; set; }
        public double Revenue { get; set; }
        public MvcHtmlString CustomFieldHtmlContent { get; set; }
    }

    public class Plan_Campaign_Program_Tactic_LineItemModel
    {
        public int PlanLineItemId { get; set; }
        public int PlanTacticId { get; set; }
        public int LineItemTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Cost { get; set; }
        public DateTime TStartDate { get; set; }
        public DateTime TEndDate { get; set; }
        public DateTime PStartDate { get; set; }
        public DateTime PEndDate { get; set; }
        public DateTime CStartDate { get; set; }
        public DateTime CEndDate { get; set; }
        public string AllocatedBy { get; set; }
        public bool IsOtherLineItem { get; set; }
    }

    public class Plan_Selector
    {
        public int PlanId { get; set; }
        public string PlanTitle { get; set; }
        public string LastUpdated { get; set; }
        public string MQLS { get; set; }
        public string Budget { get; set; }
        public string Status { get; set; }
        public bool IsPlanEditable { get; set; }
    }

    public class Plan_Tactic_Values
    {
        public int PlanTacticId { get; set; }
        public double MQL { get; set; }
        public double Revenue { get; set; }
    }

    /// <summary>
    /// Class of TacticId & Model Id Relation.
    /// </summary>
    public class TacticModelRelation
    {
        public int PlanTacticId { get; set; }
        public int ModelId { get; set; }
    }

    public class ModelConvertionRateRelation
    {
        public int ModelId { get; set; }
        public double AverageDealSize { get; set; }
        public double ConversionRate { get; set; }
    }

    public class ModelVelocityRelation
    {
        public int ModelId { get; set; }
        public double Velocity { get; set; }
    }

    public class SuggestedImprovementActivities
    {
        public int ImprovementPlanTacticId { get; set; }
        public int ImprovementTacticTypeId { get; set; }
        public string ImprovementTacticTypeTitle { get; set; }
        public double Cost { get; set; }
        public double ProjectedRevenueWithoutTactic { get; set; }
        public double ProjectedRevenueWithTactic { get; set; }
        public double ProjectedRevenueLift { get; set; }
        public double RevenueToCostRatio { get; set; }
        public bool isExits { get; set; }
        public bool isOwner { get; set; }
    }

    /// <summary>
    /// Added By: Maninder Singh Wadhva
    /// Addressed PL Ticket: 37,38,47,49
    /// Class for hypothetical model.
    /// </summary>
    public class HypotheticalModel
    {
        /// <summary>
        /// Member to hold improved metrics
        /// </summary>
        public List<ImprovedMetric> ImprovedMetrics { get; set; }
    }

    /// <summary>
    /// Added By: Maninder Singh Wadhva
    /// Addressed PL Ticket: 37,38,47,49
    /// Class for improved metric.
    /// </summary>
    public class ImprovedMetric
    {
        public int MetricId { get; set; }
        public string MetricType { get; set; }
        public string MetricCode { get; set; }
        public int? Level { get; set; }
        public double Value { get; set; }
    }

    /// Added by Bhavesh
    /// Pl Ticket 289,377,378
    public class SuggestedImprovementActivitiesConversion
    {
        public int ImprovementPlanTacticId { get; set; }
        public int ImprovementTacticTypeId { get; set; }
        public string ImprovementTacticTypeTitle { get; set; }
        public double Cost { get; set; }
        public List<ImprovedMetricWeight> ImprovedMetricsWeight { get; set; }
        public bool isExits { get; set; }
    }

    /// Added by Bhavesh
    /// Pl Ticket 289,377,378
    public class ImprovedMetricWeight
    {
        public int MetricId { get; set; }
        public int? Level { get; set; }
        public double Value { get; set; }
    }


    /// <summary>
    /// Class of TacticId & Model Id Relation.
    /// </summary>
    public class TacticPlanRelation
    {
        public int PlanTacticId { get; set; }
        public int PlanId { get; set; }
    }


    public class PlanIMPTacticListRelation
    {
        public int PlanId { get; set; }
        public List<Plan_Improvement_Campaign_Program_Tactic> ImprovementTacticList { get; set; }
    }

    public class StageRelation
    {
        public int StageId { get; set; }
        public string StageType { get; set; }
        public double Value { get; set; }
    }

    public class ModelStageRelationList
    {
        public int ModelId { get; set; }
        public List<StageRelation> StageList { get; set; }
    }

    public class ImprovementTypeWeightList
    {
        public int ImprovementTypeId { get; set; }
        public bool isDeploy { get; set; }
        public int StageId { get; set; }
        public string StageType { get; set; }
        public double Value { get; set; }
    }

    public class StageList
    {
        public int StageId { get; set; }
        public int? Level { get; set; }
        public string StageType { get; set; }
    }

    public class TacticStageValueRelation
    {
        public Plan_Campaign_Program_Tactic TacticObj { get; set; }
        public List<StageRelation> StageValueList { get; set; }
    }

    public class TacticStageValue
    {
        public Plan_Campaign_Program_Tactic TacticObj { get; set; }
        public double INQValue { get; set; }
        public double MQLValue { get; set; }
        public double CWValue { get; set; }
        public double RevenueValue { get; set; }
        public double ADSValue { get; set; }
        public double INQVelocity { get; set; }
        public double MQLVelocity { get; set; }
        public double CWVelocity { get; set; }
        public List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList { get; set; }
        public string TacticYear { get; set; }
    }

    public class PlanADSRelation
    {
        public int PlanId { get; set; }
        public double ADS { get; set; }
        public bool isImprovementExits {get; set;}
    }

    public class PlanModelRelation
    {
        public int PlanId { get; set; }
        public int ModelId { get; set; }
    }

    // Added by Sohel Pathan on 15/07/2014 for PL ticket #566
    public class BudgetAllocationModel
    {
        public double INQValue { get; set; }
        public double MQLValue { get; set; }
        public double RevenueValue { get; set; }
        public double ADSValue { get; set; }
    }

    /// <summary>
    /// Added By : Sohel Pathan
    /// Added Date : 26/08/2014
    /// Purpose : To prepare return list for the Plan, Campaign, Program, Tactic and LineItem Budget/Cost list
    /// to address PL ticket #642, #758 and #759
    /// </summary>
    public class PlanBudgetAllocationValue
    {
        public string periodTitle { get; set; }
        public string budgetValue { get; set; }
        public double campaignMonthlyBudget { get; set; }
        public double programMonthlyBudget { get; set; }
        public double remainingMonthlyBudget { get; set; }
        public string costValue { get; set; }
        public double remainingMonthlyCost { get; set; }
    }

    /// <summary>
    /// Added By : Mitesh Vaishnav for PL ticket #718
    /// Added Date : 15/09/2014
    /// class for return mapped list of custom fields
    /// </summary>
    public class CustomFieldModel
    {
        public int customFieldId { get; set; }
        public string name { get; set; }
        public string customFieldType { get; set; }
        public string description { get; set; }
        public bool isRequired { get; set; }
        public string entityType { get; set; }
        public List<CustomFieldOptionModel> option { get; set; }
        public List<string> value { get; set; }

    }

    /// <summary>
    /// Added By : Mitesh Vaishnav for PL ticket #718
    /// Added Date : 15/09/2014
    /// return mapped list for option values of dropdownlist custom fields 
    /// </summary>
    public class CustomFieldOptionModel
    {
        public int customFieldOptionId { get; set; }
        public string value { get; set; }
    }

    /// <summary>
    /// Added By : Mitesh Vaishnav
    /// Added Date : 23/01/2015
    /// return mapped list review tab custom field design
    /// </summary>
    public class CustomFieldReviewTab
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Class { get; set; }
    }

    public class ModelDateList
    {
        public int ModelId { get; set; }
        public int? ParentModelId { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }

    /// <summary>
    /// Added By : Mitesh Vaishnav for PL ticket #1074
    /// Added Date : 17/01/2015
    /// return mapped list for Advanced/Basic attributes option values of dropdownlist custom fields 
    /// </summary>
    public class CustomFieldStageWeight
    {
        public int CustomFieldId { get; set; }
        public string Value { get; set; }
        public int? Weight { get; set; }
        public string StageCode { get; set; }
        public int? StageWeight { get; set; }
    }
}
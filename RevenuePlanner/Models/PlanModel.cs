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

        public HomePlanModelHeader objplanhomemodelheader { get; set; }
    }

    public class Plan_CampaignModel
    {
        public int PlanId { get; set; }
        public int PlanCampaignId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public int OwnerId { get; set; }
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
        public string Status { get; set; }      //Added By Komal Rawal for #1292 dont apply isdeleted flag for tactics that are completed.
    }

    public class Plan_Campaign_ProgramModel
    {
        public int PlanProgramId { get; set; }
        public int PlanCampaignId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public int OwnerId { get; set; }
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
        public string Status { get; set; }      //Added By Komal Rawal for #1292 dont apply isdeleted flag for tactics that are completed.
    }

    public class Plan_Campaign_Program_TacticModel
    {
        public int PlanTacticId { get; set; }
        public int PlanProgramId { get; set; }
        public int TacticTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
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
        public double TacticBudget { get; set; }

        //Added by Brad Gray for PL ticket #1368 
 	    public string IntegrationWorkFrontProjectID { get; set; } 
    }

    public class Inspect_Popup_Plan_Campaign_Program_TacticModel
    {
        public int PlanProgramId { get; set; }
        public int PlanCampaignId { get; set; }
        public int PlanTacticId { get; set; }
        public int PlanId { get; set; } // Added by Arpita Soni for Ticket #2212 on 05/24/2016 

        public string TacticTitle { get; set; }
        public string ProgramTitle { get; set; }
        public string CampaignTitle { get; set; }

        public int TacticTypeId { get; set; }
        
        public string Description { get; set; }

        public string Owner { get; set; }
        public int OwnerId { get; set; }

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

        // Extra view bag variable
        public string CalledFromBudget { get; set; }
        public bool IsCreated { get; set; }
        public bool RedirectType { get; set; }
        public bool IsTacticAfterApproved { get; set; }
        public bool ExtIntService { get; set; }
        public string customFieldWeightage { get; set; }
        public bool IsDiffrentStageType { get; set; }
        public bool IsOwner { get; set; }
        public List<TacticType> Tactics { get; set; }
        public string Year { get; set; }
        public double? planRemainingBudget { get; set; }
        public bool IsTackticAddEdit { get; set; }
        public List<SelectListValue> PlanCampaignList { get; set; }
        public List<SelectListValue> CampaignProgramList { get; set; }
        public List<SelectListUser> OwnerList { get; set; }
        public bool IsLinkedTactic { get; set; }
        public string Status { get; set; }      //Added By Komal Rawal for #1292 dont apply isdeleted flag for tactics that are completed.
        public bool IsPackagedTactic { get; set; }
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
        public int PlanId { get; set; }  // Added by Arpita Soni for Ticket #2212 on 05/24/2016 
        //Added By Komal Rawal for #1974
        //Desc: To Enable edit owner feature from Lineitem popup.
        public bool IsLineItemAddEdit { get; set; }
        public List<SelectListUser> OwnerList { get; set; }
        public int OwnerId { get; set; }
      
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
        public int CampaignId { get; set; }
        public int Programid { get; set; }
    }
    public class Plan_Tactic_LineItem_Values
    {
        public int PlanTacticId { get; set; }
        public double Cost { get; set; }
        public int CampaignId { get; set; }
        public int Programid { get; set; }
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
        public List<TacticCustomFieldStageWeightage> TacticStageWeightages { get; set; }
        public string RoiPackageTitle { get; set; } // Add By Nishant Sheth
        public bool IsPackage { get; set; } // Add By Nishant Sheth
        public int ROIAnchorTacticId { get; set; } // Add By Nishant Sheth
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
        public double CWValue { get; set; } // added y devanshi for plangrid goal
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
        public bool isChild { get; set; }
        public int? ParentId { get; set; }
        public bool IsSelected { get; set; }
        public List<int> ParentOptionId { get; set; }
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
        public bool ChildOptionId { get; set; }
        public int customFieldOptionId { get; set; }
        public List<int?> ChildOptionIds { get; set; }
        public string value { get; set; }
        public List<int> ParentOptionId { get; set; }
        public int customFieldId { get; set; }
        //Added by Rahul shah on 05/11/2015 for PL #1731
        private bool isDefaultOption = false;
        public bool IsDefaultOption { get { return isDefaultOption; } set { isDefaultOption = value; } }
    }

    /// <summary>
    /// Added By : Mitesh Vaishnav
    /// Added Date : 23/01/2015
    /// return mapped list review tab custom field design
    /// </summary>
    public class CustomFieldReviewTab
    {
        public int CustomFieldId { get; set; }
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
        public int Weight { get; set; }
        public int CostWeight { get; set; }
    }

    /// <summary>
    /// Added By : Viral Kadiya for PL ticket #1075
    /// Added Date : 29/01/2015
    /// return Tactic related CustomField,CustomFieldOptions and respective Weightage.
    /// </summary>
    public class TacticCustomFieldStageWeightage
    {
        public int CustomFieldId { get; set; }
        public string Value { get; set; }
        public int? Weightage { get; set; }
        public int? CVRWeightage { get; set; }
        public int? CostWeightage { get; set; }
    }

    public class Plangrid
    {
        public string MQLLable { get; set; }
        public string INQLable { get; set; }
        public string CWLable { get; set; }
        public string MQLValue { get; set; }
        public string INQValue { get; set; }
        public string Revenue { get; set; }
        public string CWValue { get; set; }
        public PlanMainDHTMLXGrid PlanDHTMLXGrid { get; set; }
        public Dictionary<int,BudgetDHTMLXGridModel> lstChildPlanDHTMLXGrid { get; set; }
    }
    public class PlanImprovement
    {
        public string MQLLable { get; set; }
        public bool IsTacticExists { get; set; }
        public int Progrmas { get; set; }
        public int ImprovementPlanProgramId { get; set; }
        public double TotalCost { get; set; }
        public double TotalMqls { get; set; }
    }

    public class BudgetAccountMapping
    {
        public int Id { get; set; }
        public int Weightage { get; set; }
    }

    public class SelectListValue
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class SelectListUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class SPPlan
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
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public int ModifiedBy { get; set; }
        public string GoalType { get; set; }
        public double GoalValue { get; set; }
        public string AllocatedBy { get; set; }
        public string EloquaFolderPath { get; set; }
        public Nullable<System.DateTime> DependencyDate { get; set; }
    }
    
}
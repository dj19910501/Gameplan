﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class PlanModel
    {
        [Key]
        public int PlanId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string MQls { get; set; }

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
        public long? INQs { get; set; }
        public double? MQLs { get; set; }
        public double? Cost { get; set; }
        public DateTime? PStartDate { get; set; }
        public DateTime? PEndDate { get; set; }
        public DateTime? TStartDate { get; set; }
        public DateTime? TEndDate { get; set; }
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
        public long? INQs { get; set; }
        public double? MQLs { get; set; }
        public double? Cost { get; set; }

        public DateTime CStartDate { get; set; }
        public DateTime CEndDate { get; set; }
        public DateTime? TStartDate { get; set; }
        public DateTime? TEndDate { get; set; }
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
        public long INQs { get; set; }
        public double MQLs { get; set; }
        public double Cost { get; set; }

        public DateTime PStartDate { get; set; }
        public DateTime PEndDate { get; set; }
        public DateTime CStartDate { get; set; }
        public DateTime CEndDate { get; set; }
    }

    public class Plan_Selector
    {
        public int PlanId { get; set; }
        public string PlanTitle { get; set; }
        public string LastUpdated { get; set; }
        public string MQLS { get; set; }
        public string Budget { get; set; }
        public string Status { get; set; }
    }

    public class Plan_Tactic_MQL
    {
        public int PlanTacticId { get; set; }
        public double MQL { get; set; }
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

}
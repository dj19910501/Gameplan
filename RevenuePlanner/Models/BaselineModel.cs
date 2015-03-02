using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class BaselineModel
    {
        [Key]
        public int ModelId { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public int MarketingLeads { get; set; }
        public int MarketingDealSize { get; set; }
        public int TeleprospectingLeads { get; set; }
        public int TeleprospectingDealSize { get; set; }
        public int SalesLeads { get; set; }
        public int SalesDealSize { get; set; }
        public int? IntegrationInstanceId { get; set; }
        public int? IntegrationInstanceIdINQ { get; set; }
        public int? IntegrationInstanceIdMQL { get; set; }
        public int? IntegrationInstanceIdCW { get; set; }
        public List<ModelStage> lstmodelstage { get; set; }
        public List<ModelVersion> Versions { get; set; }
    }
    public class ModelOverView
    {
        public int ModelId { get; set; }
        public string Title { get; set; }
    }
    public class ContactInquiry
    {
        public int MarketingLeads { get; set; }
        public double MarketingDealSize { get; set; } //change the datatype from int to double
        public int TeleprospectingLeads { get; set; }
        public double TeleprospectingDealSize { get; set; } //change the datatype from int to double
        public int SalesLeads { get; set; }
        public double SalesDealSize { get; set; } //change the datatype from int to double
    }
    public class ModelVersion
    {
        public int ModelId { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
        public bool IsLatest { get; set; }
    }
    public class ModelStage
    {
        public int StageId { get; set; }
        public string ConversionTitle { get; set; }
        public string VelocityTitle { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public string Funnel { get; set; }
        public string Code { get; set; }

    }
    public class ModelFunnel
    {
        public int FunnelId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }


    }
    public class ModelResults
    {
        public int ModelFunnelId { get; set; }
        public int FunnelFieldId { get; set; }
        public string Funnel { get; set; }
        public string FieldCode { get; set; }
        public string FieldDesc { get; set; }
        public double? MarketingSourced { get; set; }
        public double? MarketingDays { get; set; }
        public double? ProspectingSourced { get; set; }
        public double? ProspectingDays { get; set; }
        public double? SalesSourced { get; set; }
        public double? SalesDays { get; set; }
        public double? BlendedTotal_v1 { get; set; }
        public double? BlendedDays_v1 { get; set; }
        public double? BlendedTotal_v2 { get; set; }
        public double? BlendedDays_v2 { get; set; }
        public string BlendedTotal_Perc { get; set; }
        public string BlendedDays_Perc { get; set; }
        public int BlendedTotalGrade { get; set; } //0-equal, 1-up, 2-down
        public int BlendedDaysGrade { get; set; } //0-equal, 1-up, 2-down
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class BoostImprovementTacticModel
    {
        public int ImprovementTacticTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Cost { get; set; }
        public string ColorCode { get; set; }
        public int ClientId { get; set; }
        public bool IsDeployed { get; set; }
        public bool IsDeleted { get; set; }
        public int MetricId { get; set; }
        public List<MetricModel> listMetricssize { get; set; }
        public List<MetricModel> listMetrics { get; set; }
        public bool IsDeployedToIntegration { get; set; }
        
    }
    public class MetricModel
    {
        public int MetricId { get; set; }
        public string MetricType { get; set; }
        public string MetricName { get; set; }
        public string MetricCode { get; set; }
        public int Level { get; set; }
        public double Weight { get; set; }
        public double ConversionValue { get; set; }
        public double VelocityValue { get; set; }
        public int MetricID_CR { get; set; }
        public int MetricID_SV { get; set; }
        public int MetricID_Size { get; set; }
    }
    public class BestInClassModel
    {
        //added by uday for PL ticket #501 removed the old ones and added the below new ones.
        public int StageID_CR { get; set; }
        public int StageID_SV { get; set; }
        public int StageID_Size { get; set; }
        public string StageName { get; set; }
        public string StageType { get; set; }
        public double ConversionValue { get; set; }
        public double VelocityValue { get; set; }
    }
}
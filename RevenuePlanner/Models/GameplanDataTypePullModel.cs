using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    /// <summary>
    /// Added By Dharmraj on 8-8-2014, Ticket #658
    /// Model for Integration Pulling Close deals
    /// </summary>
    public class GameplanDataTypePullModel : GameplanDataTypePull
    {
        public int? IntegrationInstanceDataTypeMappingPullId { get; set; }
        public int? IntegrationInstanceId { get; set; }
        public string TargetDataType { get; set; }

    }
}
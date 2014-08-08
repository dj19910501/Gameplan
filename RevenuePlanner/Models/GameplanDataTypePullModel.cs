using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class GameplanDataTypePullModel : GameplanDataTypePull
    {
        public int? IntegrationInstanceDataTypeMappingPullId { get; set; }
        public int? IntegrationInstanceId { get; set; }
        public string TargetDataType { get; set; }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class ExtrenalServiceModel
    {
        public string FieldName { get; set; }
    }
    public class ExternalField
    {
        public string TargetDataType { get; set; }
    }
    public class GameplanDataTypeModel : GameplanDataType
    {
        public int? IntegrationInstanceDataTypeMappingId { get; set; }
        public int? IntegrationInstanceId { get; set; }
        public string TargetDataType { get; set; }
    }
}
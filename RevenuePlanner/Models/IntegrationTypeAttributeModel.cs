using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class IntegrationTypeAttributeModel
    {
        [Key]
        public int IntegrationTypeAttributeId { get; set; }

        public int IntegrationTypeId { get; set; }
        public string Attribute { get; set; }
        public string AttributeType { get; set; }
        public bool IsDeleted { get; set; }

        public string Value { get; set; }
    }

    public class IntegrationInstance_AttributeModel
    {
        public int IntegrationInstanceId { get; set; }
        public int IntegrationTypeAttributeId { get; set; }
        public string Value { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
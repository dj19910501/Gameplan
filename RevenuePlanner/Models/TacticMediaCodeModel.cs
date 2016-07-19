using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class TacticMediaCodeModel
    {
        public int TacticId { get; set; }
        public int CustomFieldId { get; set; }
        public int MediaCodeId { get; set; }
        public string CustomFieldValue { get; set; }
        public string MediaCode { get; set; }
    }
    public class CustomFieldValue
    {
        public int CustomFieldId { get; set; }
        public string CustomFieldName { get; set; }
        public string CustomFieldOptionValue { get; set; }
        public string CustomFieldType { get; set; }
    }
    public class TacticMediaCodeCustomField
    {
        public int TacticId { get; set; }
        public string MediaCode { get; set; }
        public List<CustomeFieldList> CustomFieldList { get; set; }
        public int MediaCodeId { get; set; }
    }
    public class CustomeFieldList
    {
        public int CustomFieldId { get; set; }
        public string CustomFieldValue { get; set; }
    }
    public class TacticCustomfieldConfig
    {
        public int CustomFieldId { get; set; }
        public string CustomFieldName { get; set; }
        public string CustomFieldTypeName { get; set; }
        public bool IsRequired { get; set; }
        public int? Sequence { get; set; }
        public List<CustomFieldOptionList> Option { get; set; }
    }
    public class CustomFieldOptionList
    {
        public int CustomFieldOptionId { get; set; }
        public string CustomFieldOptionValue { get; set; }
    }
    public class RequriedCustomField
    {
        public string CustomFieldId { get; set; }
        public bool IsRequired { get; set; }
    }
}
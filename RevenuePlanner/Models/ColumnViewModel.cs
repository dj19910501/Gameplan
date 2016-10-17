using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class ColumnViewModel
    {

    }
    public class ColumnViewEntity
    {
        public string EntityType { get; set; }
        public bool EntityIsChecked { get; set; }
        public List<ColumnViewAttribute> AttributeList { get; set; }
    }
    public class ColumnViewAttribute
    {
        public string CustomFieldId { get; set; }
        public string CutomfieldName { get; set; }
        public bool IsChecked { get; set; }
        public int ParentID { get; set; }
        public string FieldType { get; set; }

    }
    public class AttributeDetail
    {
        public string AttributeType { get; set; }
        public string AttributeId { get; set; }
        public string ColumnOrder { get; set; }
    }
    public class CustomAttribute
    {
        public string CustomFieldId { get; set; }
        public string CutomfieldName { get; set; }
        public int ParentID { get; set; }
        public string EntityType { get; set; }
        public string CustomfiledType { get; set; }
        public bool IsRequired { get; set; }
        public string FieldType { get; set; }
    }
    public class CustomfieldOption
    {
        public int CustomFieldId { get; set; }
        public int CustomFieldOptionId { get; set; }
        public string OptionValue { get; set; }

    }
}
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
        public List<ColumnViewAttribute> AttributeList { get; set; }
    }
    public class ColumnViewAttribute
    {
        public string CustomFieldId { get; set; }
        public string CutomfieldName { get; set; }
        public int ParentID { get; set; }
    }
    public class AttributeDetail
    {
        public string AttributeType { get; set; }
        public string AttributeId { get; set; }
    }
}
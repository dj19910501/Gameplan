//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RevenuePlanner.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DynamicDimension
    {
        public int id { get; set; }
        public string TableName { get; set; }
        public Nullable<int> Dimensions { get; set; }
        public string DimensionTableName { get; set; }
        public Nullable<bool> ComputeAllValues { get; set; }
        public string DimensionValueTableName { get; set; }
        public Nullable<bool> ContainsDateDimension { get; set; }
    }
}

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
    
    public partial class DimTime
    {
        public int id { get; set; }
        public System.DateTime DateTime { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int Month { get; set; }
        public string Monthname { get; set; }
        public int Day { get; set; }
    }
}

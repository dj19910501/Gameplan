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
    
    public partial class Report_Intergration_Conf
    {
        public long Id { get; set; }
        public string TableName { get; set; }
        public string IdentifierColumn { get; set; }
        public string IdentifierValue { get; set; }
        public int ClientId { get; set; }
    }
}

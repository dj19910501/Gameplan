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
    
    public partial class MenuItem
    {
        public int Id { get; set; }
        public Nullable<int> HomepageId { get; set; }
        public Nullable<int> DashboardId { get; set; }
        public int DisplayOrder { get; set; }
    
        public virtual Dashboard Dashboard { get; set; }
        public virtual Homepage Homepage { get; set; }
    }
}

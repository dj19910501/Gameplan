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
    
    public partial class LineItemType
    {
        public LineItemType()
        {
            this.Plan_Campaign_Program_Tactic_LineItem = new HashSet<Plan_Campaign_Program_Tactic_LineItem>();
        }
    
        public int LineItemTypeId { get; set; }
        public int ModelId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
    
        public virtual ICollection<Plan_Campaign_Program_Tactic_LineItem> Plan_Campaign_Program_Tactic_LineItem { get; set; }
        public virtual Model Model { get; set; }
    }
}

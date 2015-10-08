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
    
    public partial class Budget_Detail
    {
        public Budget_Detail()
        {
            this.Budget_DetailAmount = new HashSet<Budget_DetailAmount>();
            this.LineItem_Budget = new HashSet<LineItem_Budget>();
        }
    
        public int Id { get; set; }
        public int BudgetId { get; set; }
        public string Name { get; set; }
        public Nullable<int> ParentId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
    
        public virtual Budget Budget { get; set; }
        public virtual ICollection<Budget_DetailAmount> Budget_DetailAmount { get; set; }
        public virtual ICollection<LineItem_Budget> LineItem_Budget { get; set; }
    }
}

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
    
    public partial class Plan_Campaign_Program_Tactic_LineItem
    {
        public Plan_Campaign_Program_Tactic_LineItem()
        {
            this.LineItem_Budget = new HashSet<LineItem_Budget>();
            this.Plan_Campaign_Program_Tactic_LineItem_Actual = new HashSet<Plan_Campaign_Program_Tactic_LineItem_Actual>();
            this.Plan_Campaign_Program_Tactic_LineItem_Cost = new HashSet<Plan_Campaign_Program_Tactic_LineItem_Cost>();
            this.TransactionLineItemMappings = new HashSet<TransactionLineItemMapping>();
        }
    
        public int PlanLineItemId { get; set; }
        public int PlanTacticId { get; set; }
        public Nullable<int> LineItemTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public double Cost { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> LinkedLineItemId { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
    
        public virtual ICollection<LineItem_Budget> LineItem_Budget { get; set; }
        public virtual LineItemType LineItemType { get; set; }
        public virtual Plan_Campaign_Program_Tactic Plan_Campaign_Program_Tactic { get; set; }
        public virtual ICollection<Plan_Campaign_Program_Tactic_LineItem_Actual> Plan_Campaign_Program_Tactic_LineItem_Actual { get; set; }
        public virtual ICollection<Plan_Campaign_Program_Tactic_LineItem_Cost> Plan_Campaign_Program_Tactic_LineItem_Cost { get; set; }
        public virtual ICollection<TransactionLineItemMapping> TransactionLineItemMappings { get; set; }
    }
}

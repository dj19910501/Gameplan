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
    
    public partial class Budget_ColumnSet
    {
        public Budget_ColumnSet()
        {
            this.Budget_Columns = new HashSet<Budget_Columns>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public int ClientId { get; set; }
        public int CreatedBy { get; set; }
    
        public virtual ICollection<Budget_Columns> Budget_Columns { get; set; }
    }
}

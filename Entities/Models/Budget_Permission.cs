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
    
    public partial class Budget_Permission
    {
        public int Id { get; set; }
        public System.Guid UserId { get; set; }
        public int BudgetDetailId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public int PermisssionCode { get; set; }
    
        public virtual Budget_Detail Budget_Detail { get; set; }
    }
}
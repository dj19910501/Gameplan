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
    
    public partial class Field
    {
        public Field()
        {
            this.Funnel_Field = new HashSet<Funnel_Field>();
        }
    
        public int FieldId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
    
        public virtual ICollection<Funnel_Field> Funnel_Field { get; set; }
    }
}

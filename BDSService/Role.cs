//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BDSService
{
    using System;
    using System.Collections.Generic;
    
    public partial class Role
    {
        public Role()
        {
            this.Application_Role = new HashSet<Application_Role>();
            this.Role_Activity_Permission = new HashSet<Role_Activity_Permission>();
            this.Role_Permission = new HashSet<Role_Permission>();
        }
    
        public System.Guid RoleId { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.Guid> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public string ColorCode { get; set; }
        public Nullable<System.Guid> ClientId { get; set; }
    
        public virtual ICollection<Application_Role> Application_Role { get; set; }
        public virtual ICollection<Role_Activity_Permission> Role_Activity_Permission { get; set; }
        public virtual ICollection<Role_Permission> Role_Permission { get; set; }
        public virtual Client Client { get; set; }
    }
}

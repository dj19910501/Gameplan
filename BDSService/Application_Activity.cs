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
    
    public partial class Application_Activity
    {
        public Application_Activity()
        {
            this.Role_Activity_Permission = new HashSet<Role_Activity_Permission>();
            this.User_Activity_Permission = new HashSet<User_Activity_Permission>();
        }
    
        public int ApplicationActivityId { get; set; }
        public System.Guid ApplicationId { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string ActivityTitle { get; set; }
        public string Code { get; set; }
        public System.DateTime CreatedDate { get; set; }
    
        public virtual Application Application { get; set; }
        public virtual ICollection<Role_Activity_Permission> Role_Activity_Permission { get; set; }
        public virtual ICollection<User_Activity_Permission> User_Activity_Permission { get; set; }
    }
}

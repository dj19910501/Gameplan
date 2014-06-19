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
    
    public partial class Application
    {
        public Application()
        {
            this.Application_Activity = new HashSet<Application_Activity>();
            this.Application_Role = new HashSet<Application_Role>();
            this.Menu_Application = new HashSet<Menu_Application>();
            this.User_Application = new HashSet<User_Application>();
        }
    
        public System.Guid ApplicationId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.Guid> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public string ReleaseVersion { get; set; }
    
        public virtual ICollection<Application_Activity> Application_Activity { get; set; }
        public virtual ICollection<Application_Role> Application_Role { get; set; }
        public virtual ICollection<Menu_Application> Menu_Application { get; set; }
        public virtual ICollection<User_Application> User_Application { get; set; }
    }
}

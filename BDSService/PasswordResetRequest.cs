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
    
    public partial class PasswordResetRequest
    {
        public System.Guid PasswordResetRequestId { get; set; }
        public System.Guid UserId { get; set; }
        public bool IsUsed { get; set; }
        public int AttemptCount { get; set; }
        public System.DateTime CreatedDate { get; set; }
    
        public virtual User User { get; set; }
    }
}

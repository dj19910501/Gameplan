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
    
    public partial class IntegrationWorkFrontTacticSetting
    {
        public int Id { get; set; }
        public int TacticId { get; set; }
        public bool IsDeleted { get; set; }
        public string TacticApprovalObject { get; set; }
    
        public virtual Plan_Campaign_Program_Tactic Plan_Campaign_Program_Tactic { get; set; }
    }
}

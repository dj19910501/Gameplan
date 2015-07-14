using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class PermissionModel
    {
        public bool IsPlanCreateAllAuthorized { get; set; }
        public bool IsTacticActualsAddEditAuthorized{get;set;}
        public bool IsPlanEditable { get; set; }
        public bool IsPlanCreateAll { get; set; }
        public bool IsTacticAllowForSubordinates { get; set; }
        public bool IsPlanEditSubordinatesAuthorized { get; set; }
        public bool IsPlanEditAllAuthorized { get; set; }
        public bool IsPlanCreateAuthorized { get; set; }
        public bool IsPlanDefinationDisable { get; set; }
    }
}
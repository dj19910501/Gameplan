using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class PermissionModel
    {
        public bool IsPlanCreateAllAuthorized { get; set; }
        public bool IsPlanCreateAuthorized { get; set; }
        public bool IsPlanCreateAll { get; set; }
        public string InspectMode { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class ChangeLog_ViewModel
    {
        public string ComponentTitle { get; set; }
        public string ComponentType { get; set; }
        public string Action { get; set; }
        public string User { get; set; }
        public string DateStamp { get; set; }
        public string ActionSuffix { get; set; }
    }
}
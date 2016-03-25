using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class GetData
    {
        public string viewBy { get; set; }
        public string planId { get; set; }
        public string timeFrame { get; set; }
        public string customFieldIds { get; set; }
        public string ownerIds { get; set; }
        public string activeMenu { get; set; }
        public bool getViewByList { get; set; }
        public string TacticTypeid { get; set; }
        public string StatusIds { get; set; }
        public bool isupdate { get; set; }

    }
}
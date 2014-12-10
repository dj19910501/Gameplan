﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Eloqua
{
    class EloquaModel
    {

    }

    class InstanceURL
    {
        public URLS urls { get; set; }
    }

    class URLS
    {
        public APIS apis { get; set; }
    }

    class APIS
    {
        public Rest rest { get; set; }
    }

    class Rest
    {
        public string standard { get; set; }
    }

    class EloquaCampaign
    {
        public string id { get; set; }

        /// Modified By: Maninder Singh
        /// Modified Date: 08/20/2014
        /// Ticket #717 Pulling from Eloqua - Actual Cost 
        public double actualCost { get; set; }

        public int folderId { get; set; }
    }

    public class FieldValue
    {
        public string type { get; set; }
        public string id { get; set; }
        public string value { get; set; }
    }

    public class EloquaResponseModel
    {
        public string eloquaTacticId { get; set; }
        public string externalTacticId { get; set; }
        public DateTime peroid { get; set; }
        public int responseCount { get; set; }
    }
}

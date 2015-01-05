using System;
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
        
        /// Added By: Viral Kadiya
        /// Date: 01/05/2015
        /// Ticket #1072 Integration: Multiple integration within same tactic
        public string crmId { get; set; }
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

    public class ContactListDetailModel
    {
        public string type { get; set; }
        public string id { get; set; }
        public string createdAt { get; set; }
        public string createdBy { get; set; }
        public string depth { get; set; }
        public string description { get; set; }
        public string folderId { get; set; }
        public string name { get; set; }
        public string permissions { get; set; }
        public string updatedAt { get; set; }
        public string count { get; set; }
        public string dataLookupId { get; set; }
        public string scope { get; set; }
        public string membershipDeletions { get; set; }
        public string membershipAdditions { get; set; }
    }

    public class ContactListContactModel
    {
        public List<elements> elements { get; set; }
        public string page { get; set; }
        public string pageSize { get; set; }
        public string total { get; set; }
    }

    public class elements
    {
        public string type { get; set; }
        public string contactId { get; set; }
        public string CampaignId { get; set; }
        public string MQLDate { get; set; }
        public DateTime peroid { get; set; }
    }

    public class CRM_EloquaMapping
    {
        public string CRMId { get; set; }
        public string EloquaId { get; set; }
    }
}

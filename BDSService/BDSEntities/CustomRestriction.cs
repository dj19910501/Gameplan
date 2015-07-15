using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace BDSService.BDSEntities
{
    public class CustomRestriction
    {
        [DataMember]
        public int CustomRestrictionId { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public string CustomField { get; set; }

        [DataMember]
        public string CustomFieldId { get; set; }

        [DataMember]
        public int Permission { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public Guid CreatedBy { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BDSService.BDSEntities
{
    [DataContract]
    public class UserApplicationPermission
    {
        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public int ApplicationActivityId { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public Guid CreatedBy { get; set; }

    }
}
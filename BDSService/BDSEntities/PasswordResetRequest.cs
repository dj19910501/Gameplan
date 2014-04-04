using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BDSService.BDSEntities
{
    [DataContract]
    public class PasswordResetRequest
    {
        [DataMember]
        public Guid PasswordResetRequestId { get; set; }
        [DataMember]
        public Guid UserId { get; set; }
        [DataMember]
        public bool IsUsed { get; set; }
        [DataMember]
        public int AttemptCount { get; set; }
        [DataMember]
        public System.DateTime CreatedDate { get; set; }
    }
}
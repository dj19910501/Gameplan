using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BDSService.BDSEntities
{
    [DataContract]
    public class Role
    {
        [DataMember]
        public Guid RoleId { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public DateTime? CreatedDate { get; set; }
        [DataMember]
        public Guid? CreatedBy { get; set; }
        [DataMember]
        public DateTime? ModifiedDate { get; set; }
        [DataMember]
        public Guid? ModifiedBy { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BDSService.BDSEntities
{
    [DataContract]
    public class Permission
    {
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public int PermissionCode { get; set; }
    }
}

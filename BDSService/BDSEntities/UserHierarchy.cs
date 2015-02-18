using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace BDSService.BDSEntities
{
    [DataContract]
    public class UserHierarchy
    {
        [DataMember]
        public Guid UserId { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public Guid RoleId { get; set; }
        [DataMember]
        public string RoleTitle { get; set; }
        [DataMember]
        public string ColorCode { get; set; }
        [DataMember]
        public string JobTitle { get; set; }
        [DataMember]
        public string Phone { get; set; }
        [DataMember]
        public Guid? ManagerId { get; set; }
    }
}
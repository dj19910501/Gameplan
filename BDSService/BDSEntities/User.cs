using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BDSService.BDSEntities
{
    [DataContract]
    public class User
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
        public string Password { get; set; }
        [DataMember]
        public byte[] ProfilePhoto { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string JobTitle { get; set; }
        [DataMember]
        public Guid RoleId { get; set; }
        [DataMember]
        public string RoleCode { get; set; }
        [DataMember]
        public string RoleTitle { get; set; }
        [DataMember]
        public System.Guid BusinessUnitId { get; set; }
        [DataMember]
        public Guid GeographyId { get; set; }
        [DataMember]
        public string Client { get; set; }
        [DataMember]
        public Guid ClientId { get; set; }
        [DataMember]
        public bool IsSystemAdmin { get; set; }
        [DataMember]
        public bool IsClientAdmin { get; set; }
        [DataMember]
        public bool IsDirector { get; set; }
        [DataMember]
        public bool IsPlanner { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public DateTime? LastLoginDate { get; set; }
        [DataMember]
        public Nullable<int> SecurityQuestionId { get; set; }
        [DataMember]
        public string SecurityQuestion { get; set; }
        [DataMember]
        public string Answer { get; set; }
        // Start - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
        [DataMember]
        public Guid? ManagerId { get; set; }
        [DataMember]
        public string ManagerName { get; set; }
        [DataMember]
        public bool IsManager { get; set; }
        [DataMember]
        public Guid? NewManagerId { get; set; }
        [DataMember]
        public string Phone { get; set; }
        // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
    }
}

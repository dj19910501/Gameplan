using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BDSService.BDSEntities
{
    //added by uday as per DB changes for #513
    [DataContract]
    public class ApplicationActivity
    {
        [DataMember]
        public int ApplicationActivityId { get; set; }
        [DataMember]
        public Guid ApplicationId { get; set; }
        [DataMember]
        public int? ParentId { get; set; }
        [DataMember]
        public string ActivityTitle { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public DateTime? CreatedDate { get; set; }

    }
}
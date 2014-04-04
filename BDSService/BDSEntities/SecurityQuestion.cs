using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BDSService.BDSEntities
{
    public class SecurityQuestion
    {
        [DataMember]
        public int SecurityQuestionId { get; set; }
        [DataMember]
        public string SecurityQuestion1 { get; set; } 
    }
}
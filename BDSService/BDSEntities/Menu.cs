using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BDSService.BDSEntities
{
    [DataContract]
    public class Menu
    {
        [DataMember]
        public int MenuApplicationId { get; set; }
        [DataMember]
        public Nullable<int> ParentApplicationId { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool IsDisplayInMenu { get; set; }
        [DataMember]
        public int SortOrder { get; set; }
        [DataMember]
        public string ControllerName { get; set; }
        [DataMember]
        public string ActionName { get; set; }
        [DataMember]
        public bool IsEnable { get; set; }
    }
}

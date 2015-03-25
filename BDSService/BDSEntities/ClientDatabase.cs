using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BDSService.BDSEntities
{
    public class ClientDatabase
    {
        public int ClientDatabaseID { get; set; }
        public Guid ClientID { get; set; }
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
        public class OrganizationModel
        {
            public Guid UserId { get; set; }
            public Guid ClientId { get; set; }
            public Guid BusinessUnitId { get; set; }
            public Guid GeographyId { get; set; }
            public string Client { get; set; }
            public string DisplayName { get; set; }
            public string FirstName { get; set; }
            public string JobTitle { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public Guid RoleId { get; set; }
            public string RoleCode { get; set; }
            public string RoleTitle { get; set; }
        }

        public class RoleModel
        {
            public Guid RoleId { get; set; }
            public string RoleCode { get; set; }
            public string RoleTitle { get; set; }
        }

        public class ActivityModel
        {
            public Guid ApplicationId { get; set; }
        }

}
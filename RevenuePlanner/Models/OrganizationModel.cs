using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
        public class OrganizationModel
        {
            public int UserId { get; set; }
            public int ClientId { get; set; }
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

        public class UserHierarchyModel
        {
            public int UserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public Guid RoleId { get; set; }
            public string RoleTitle { get; set; }
            public string ColorCode { get; set; }
            public string JobTitle { get; set; }
            public string Phone { get; set; }
            public Guid? ManagerId { get; set; }
            public IList<UserHierarchyModel> subUsers { get; set; }
            public Guid UserGuid { get; set; }
        }

}
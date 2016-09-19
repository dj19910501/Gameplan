using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RevenuePlanner.Models
{
    /// <summary>
    /// Added By: Mitesh Vaishnav
    /// Addressed PL Ticket: 521
    /// Class for User's activity permissions .
    /// </summary>
    public class UserActivityPermissionModel
    {

        public int ApplicationActivityId { get; set; }

        public Guid ApplicationId { get; set; }

        public int? ParentId { get; set; }

        public string Title { get; set; }

        public string ItemCode { get; set; } //Added by Rahul Shah on 06/10/2015 for PL#1638

        public DateTime? CreatedDate { get; set; }

        public int UserId { get; set; }

        public DateTime UserCreatedDate { get; set; }

        public Guid UserCreatedBy { get; set; }

        public string Permission { get; set; }
    }

    /// <summary>
    /// Added By: Dharmraj Mangukiya
    /// Addressed PL Ticket: 538
    /// Class for User's custom restriction.
    /// </summary>
    public class UserCustomRestrictionModel
    {
        public string CustomField { get; set; }

        public string CustomFieldId { get; set; }

        public int Permission { get; set; }
    }
}
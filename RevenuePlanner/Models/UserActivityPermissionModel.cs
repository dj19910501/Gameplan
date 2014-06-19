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

        public DateTime? CreatedDate { get; set; }

        public Guid UserId { get; set; }

        public DateTime UserCreatedDate { get; set; }

        public Guid UserCreatedBy { get; set; }

        public string Permission { get; set; }
    }
}
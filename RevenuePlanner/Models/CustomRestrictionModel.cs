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
    /// Class for User's custom restrictions and permissions .
    /// </summary>
    public class CustomRestrictionModel
    {

        public int CustomRestrictionId { get; set; }

        public int UserId { get; set; }

        public string CustomField { get; set; }

        public string CustomFieldId { get; set; }

        public int Permission { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedBy { get; set; }

        public string permissiontext { get; set; }

        public string Title { get; set; }
    }

    public class CustomFieldFilter
    {
        public int CustomFieldId { get; set; }
        public string OptionId { get; set; }
    }
}
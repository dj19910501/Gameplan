using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Models
{
    // For Integration Type
    public class IntegrationTypeModel
    {
        public int IntegrationTypeId { get; set; }

        [Display(Name = "Integration Type")]
        [Required]
        [MaxLength(250, ErrorMessage = "Title cannot be more than 250 characters.")]
        public string Title { get; set; }

        [Display(Name = "API URL")]
        [MaxLength(1000, ErrorMessage = "APIURL cannot be more than 1000 characters.")]
        public string APIURL { get; set; }

        [Display(Name = "API Version")]
        [MaxLength(25, ErrorMessage = "APIVersion cannot be more than 25 characters.")]
        public string APIVersion { get; set; }

        [Display(Name = "Description")]
        [MaxLength(4000, ErrorMessage = "Description cannot be more than 4000 characters.")]
        public string Description { get; set; }

        [Display(Name = "IsDeleted")]
        [Required]
        public bool IsDeleted { get; set; }

        [Display(Name = "Integration Type Code")]
        [Required]
        [MaxLength(250, ErrorMessage = "Code cannot be more than 250 characters.")]
        public string Code { get; set; }

        public List<SelectListItem> plans { get; set; }
    }
}
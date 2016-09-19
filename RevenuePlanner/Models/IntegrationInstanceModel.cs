using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    // For Intefration Instance
    public class IntegrationInstanceModel
    {
        [Required]
        public int IntegrationInstanceId { get; set; }

        [Required]
        public int IntegrationTypeId { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Display(Name = "Instance Name")]
        [Required]
        [MaxLength(250, ErrorMessage = "Instance name cannot be more than 250 characters.")]
        public string Instance { get; set; }

        [Display(Name = "Username")]
        [Required]
        [RegularExpression("^[A-Za-z0-9_\\+-]+(\\.[A-Za-z0-9_\\+-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9-]+)*\\.([A-Za-z]{2,4})$", ErrorMessage = "Not a valid Username")]
        [MaxLength(250, ErrorMessage = "Username cannot be more than 250 characters.")]
        public string Username { get; set; }

        [Display(Name = "Password")]
        [Required]
        [MaxLength(250, ErrorMessage = "Password cannot be more than 250 characters.")]
        public string Password { get; set; }

        [Display(Name = "Is Integration Active ?")]
        [Required]
        public bool IsActive { get; set; }

        [Display(Name = "LastSyncDate")]
        [Required]
        public DateTime LastSyncDate { get; set; }

        [Display(Name = "LastSyncStatus")]
        [Required]
        public string LastSyncStatus { get; set; }

        [Display(Name = "IsImportActuals?")]
        [Required]
        public bool IsImportActuals { get; set; }

        [Display(Name = "Title")]
        [Required]
        [MaxLength(250, ErrorMessage = "Title cannot be more than 250 characters.")]
        public bool IsDeleted { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public Guid CreatedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public Guid ModifiedBy { get; set; }
    }
}
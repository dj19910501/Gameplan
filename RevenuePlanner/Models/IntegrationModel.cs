using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class IntegrationModel
    {
        public int IntegrationInstanceId { get; set; }

        public int IntegrationTypeId { get; set; }

        public Guid ClientId { get; set; }

        [Display(Name = "Instance Name")]
        [Required]
        [MaxLength(250, ErrorMessage = "Instance name cannot be more than 250 characters.")]
        public string Instance { get; set; }

        [Display(Name = "Username")]
        [Required]
        //[RegularExpression("^[A-Za-z0-9_\\+-]+(\\.[A-Za-z0-9_\\+-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9-]+)*\\.([A-Za-z]{2,4})$", ErrorMessage = "Not a valid Username")]
        [MaxLength(250, ErrorMessage = "Username cannot be more than 250 characters.")]
        public string Username { get; set; }

        [Display(Name = "Password")]
        [Required]
        [MaxLength(250, ErrorMessage = "Password cannot be more than 250 characters.")]
        public string Password { get; set; }

        [Display(Name = "Is Integration Active ?")]
        public bool IsActive { get; set; }

        [Display(Name = "LastSyncDate")]
        public DateTime LastSyncDate { get; set; }

        [Display(Name = "LastSyncStatus")]
        public string LastSyncStatus { get; set; }

        [Display(Name = "IsImportActuals?")]
        public bool IsImportActuals { get; set; }

        [Display(Name = "Delete Integration?")]
        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public Guid ModifiedBy { get; set; }

        public IntegrationTypeModel IntegrationType { get; set; }

        public List<IntegrationTypeAttributeModel> IntegrationTypeAttributes { get; set; }
        public List<IntegrationInstance_AttributeModel> IntegrationInstance_Attribute { get; set; }

        public SyncFrequencyModel SyncFrequency { get; set; }
    }
}
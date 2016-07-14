using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class ExtrenalServiceModel
    {
        public string FieldName { get; set; }
    }
    public class ExternalField
    {
        public string TargetDataType { get; set; }
    }
    public class GameplanDataTypeModel : GameplanDataType
    {
        public int? IntegrationInstanceDataTypeMappingId { get; set; }
        public int? IntegrationInstanceId { get; set; }
        public string TargetDataType { get; set; }
        public bool IsCustomField { get; set; }
    }
    //Manoj Start PL#579 Server Configuration
    public class IntegrationInstanceExternalServerModel
    {

        public int IntegrationInstanceExternalServerId { get; set; }

        public int IntegrationInstanceId { get; set; }

        [Required]
        [Display(Name = "Server Name")]
        [MaxLength(255, ErrorMessage = "Server Name cannot be more than 255 characters.")]
        public string SFTPServerName { get; set; }

        [Required]
        [MaxLength(1000, ErrorMessage = "File Location cannot be more than 1000 characters.")]
        [Display(Name = "File Location")]
        public string SFTPFileLocation { get; set; }

        [Required]
        [Display(Name = "User Name")]
        [MaxLength(255, ErrorMessage = "User Name cannot be more than 255 characters.")]
        public string SFTPUserName { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [MaxLength(255, ErrorMessage = "Password cannot be more than 255 characters.")]
        public string SFTPPassword { get; set; }

        [Display(Name = "Port")]
        [MaxLength(4, ErrorMessage = "Port cannot be more than 4 characters.")]
        public string SFTPPort { get; set; }

    }
    //Manoj End PL#579 Server Configuration
    public class CustomDashboardModel
    {
        public string DisplayName { get; set; }

        public string PermissionType { get; set; }

        public int DashboardId { get; set; }
    }
}
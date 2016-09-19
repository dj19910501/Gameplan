using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Models
{
    public class IntegrationModel
    {
        public int IntegrationInstanceId { get; set; }

        public int IntegrationTypeId { get; set; }

        public int ClientId { get; set; }

        [AllowHtml]
        [Display(Name = "Instance Name")]
        [Required]
        [MaxLength(250, ErrorMessage = "Instance name cannot be more than 250 characters.")]
        //[RegularExpression("^[^<>~%^;/|]+", ErrorMessage = "^<>~%;/| characters are not allowed in Instance Name.")]
        public string Instance { get; set; }

        [AllowHtml]
        [Display(Name = "User ID")]
        [Required]
        //[RegularExpression("^[^<>~%^;/|]+", ErrorMessage = "^<>~%;/| characters are not allowed in Username.")]
        [MaxLength(250, ErrorMessage = "Username cannot be more than 250 characters.")]
        public string Username { get; set; }

        [Display(Name = "Password")]
        [Required]
        //[RegularExpression("^[^<>~%^;/|]+", ErrorMessage = "^<>~%;/| characters are not allowed in Password.")]
        [MaxLength(250, ErrorMessage = "Password cannot be more than 250 characters.")]
        public string Password { get; set; }

        [Display(Name = "Is Integration Active ?")]
        public bool IsActive { get; set; }

        [Display(Name = "LastSyncDate")]
        public DateTime LastSyncDate { get; set; }

        [Display(Name = "LastSyncStatus")]
        public string LastSyncStatus { get; set; }

        [Display(Name = "Import Actuals")]
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

        public bool IsActiveStatuChanged { get; set; }

        // Added by Sohel Pathan on 05/08/2014 for PL ticket #656 and #681
        public IList<GameplanDataTypeModel> GameplanDataTypeModelList { get; set; }
        // Added by Sohel Pathan on 05/08/2014 for PL ticket #656 and #681

        //Manoj Start PL#579 Server Configuration
        public IntegrationInstanceExternalServerModel ExternalServer { get; set; }
        //Manoj End PL#579 Server Configuration
        //Dharmraj Start PL#658 Integration - UI - Pulling Revenue - Salesforce.com
        public IList<GameplanDataTypePullModel> GameplanDataTypePullModelList { get; set; }
        //Dharmraj End PL#658 Integration - UI - Pulling Revenue - Salesforce.com

        //Dharmraj Start PL#680 Integration - UI - Pull responses from Salesforce
        public IList<GameplanDataTypePullModel> GameplanDataTypePullRevenueModelList { get; set; }
        //Dharmraj End PL#680 Integration - UI - Pull responses from Salesforce

        //Brad Gray Start PL#1367 Integration - UI - Sync Data with WorkFront; commented out as unneeded & unused by Brad Gray 26 March 2016
        //public IList<GameplanDataTypePullModel> GameplanDataTypeWorkFrontModelList { get; set; }
        //Brad Gray End PL#1367 Integration - UI - Sync Data with WorkFront

        //Brad Gray Start 26 March 2016 to persist list of readonly custom fields, PL#2084 
        public IList<GameplanDataTypeModel> CustomReadOnlyDataModelList { get; set; }
        //Brad Gray End 26 March 2016

        //// Start - Added by Sohel Pathan on 22/12/2014 - PL#1061 Integration - UI - Pull MQL responses from Eloqua
        public IList<GameplanDataTypePullModel> GameplanDataTypePullMQLModelList { get; set; }
        //// End - Added by Sohel Pathan on 22/12/2014 - PL#1061 Integration - UI - Pull MQL responses from Eloqua
    }

    //Added by Mitesh Vaishnav for PL ticket #659
    public class IntegrationSelectionModel
    {
        public string Setup { get; set; }
        public string Instance { get; set; }
        public string IntegrationType { get; set; }
        public string LastSync { get; set; }
    }
    //End : Added by Mitesh Vaishnav for PL ticket #659

    //Start :Added by Pratik for PL ticket #998
    public class IntegrationPlanList
    {
        public string Year { get; set; }
        public int PlanId { get; set; }
        public string PlanTitle { get; set; }
        public string FolderPath { get; set; }
        public int Permission { get; set; }
        public Guid BUId { get; set; }
        public string CampaignfolderValue { get; set; }
    }
    //End :Added by Pratik for PL ticket #998
    //Start :Added by Rahul for PL ticket #2017
    public class lstInstance
    {
        public string InstanceName { get; set; }
        public int InstanceId { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
    }
    //End :Added by Rahul for PL ticket #2017
    //Start :Added by Viral for PL ticket #1449
    public class IntegrationInstanceListing
    {
        public int IntegrationInstanceId { get; set; }
        public int IntegrationTypeId { get; set; }
        public string Instance { get; set; }
        public string Provider { get; set; }
        public string LastSyncStatus { get; set; }
        public string LastSyncDate { get; set; }
        public string AutoLastSyncDate { get; set; }
        public string ForceSyncUser { get; set; }
    }
    //End :Added by Pratik for PL ticket #998
}
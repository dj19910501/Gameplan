using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Models
{
    public class UserModel
    {
        [Key]
        public Guid UserId { get; set; }

        //[RegularExpression("^[^<>~%^;/|]+", ErrorMessage = "^<>~%;/| characters are not allowed in First Name.")]
        [AllowHtml]
        [Display(Name = "First Name")]
        [MaxLength(50, ErrorMessage = "Maximum 50 characters are allowed in First Name.")]
        [Required]
        
        public string FirstName { get; set; }

        //[RegularExpression("^[^<>~%^;/|]+", ErrorMessage = "^<>~%;/| characters are not allowed in Last Name.")]
        [AllowHtml]
        [Display(Name = "Last Name")]
        [MaxLength(50, ErrorMessage = "Maximum 50 characters are allowed in Last Name.")]
        [Required]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Email")]
        [RegularExpression("^[A-Za-z0-9_\\+-]+(\\.[A-Za-z0-9_\\+-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9-]+)*\\.([A-Za-z]{2,4})$", ErrorMessage = "Not a valid Email.")]
        [MaxLength(100, ErrorMessage = "Email cannot contain more than 100 characters.")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        //[MaxLength(255, ErrorMessage = "Password cannot contain more than 255 characters.")]
        [MaxLength(50, ErrorMessage = "Password cannot be more than 50 characters.")] //Added By Maitri Gandhi on 15/4/2016
        [MinLength(8, ErrorMessage = "Password should be atleast 8 characters.")]
        [RegularExpression(@"(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[@#$%^&*_+|~!?\.=`:'\(\)\-\\\{\}\[\]\<\>\//]).*$", ErrorMessage = " ")]    //Added By Maitri Gandhi on 8/4/2016 for #2105.  Modified for #2131
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        //[MaxLength(255, ErrorMessage = "Confirm Password cannot contain more than 255 characters.")]
        [MaxLength(50, ErrorMessage = "Password cannot be more than 50 characters.")] //Added By Maitri Gandhi on 15/4/2016
        [MinLength(8, ErrorMessage = "Confirm Password should be atleast 8 characters.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Role")]
        public Guid RoleId { get; set; }

        [Display(Name = "Role")]
        public string RoleCode { get; set; }

        [Display(Name = "Role")]
        public string RoleTitle { get; set; }

        [Display(Name = "Organization")]
        public Guid ClientId { get; set; }

        [Display(Name = "Organization")]
        public string Client { get; set; }

        [Display(Name = "Profile Photo")]
        public byte[] ProfilePhoto { get; set; }

        [Display(Name = "Display Name")]
        //[RegularExpression("^[^<>~%^;/|]+", ErrorMessage = "^<>~%;/| characters are not allowed in Display Name.")]
        [MaxLength(255, ErrorMessage = "Display Name cannot contain more than 255 characters.")]
        public string DisplayName { get; set; }

        [AllowHtml]
        [Required]
        [Display(Name = "Job Title")]
        //[RegularExpression("^[^<>~%^;/|]+", ErrorMessage = "^<>~%;/| characters are not allowed in Job Title.")]
        [MaxLength(50, ErrorMessage = "Job Title cannot contain more than 50 characters.")]
        public string JobTitle { get; set; }

        public bool IsSystemAdmin { get; set; }

        public bool IsClientAdmin { get; set; }

        public bool IsDirector { get; set; }

        public bool IsPlanner { get; set; }

        [Display(Name = "Delete User?")]
        public string IsDeleted { get; set; }

        // Start - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
        public Guid? ManagerId { get; set; }

        public string ManagerName { get; set; }

        public bool IsManager { get; set; }

        public Guid? NewManagerId { get; set; }

        [Display(Name = "Phone")]
        [RegularExpression(@"^[0-9-+ #*]+", ErrorMessage = "Please enter proper phone number.")]
        public string Phone { get; set; }
        // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517

        [Display(Name = "Currency")]
        public string PreferredCurrencyCode { get; set; }
    }

    //For User Notification
    public class UserNotification
    {
        public int NotificationId { get; set; }

        public string NotificationTitle { get; set; }

        public string NotificationType { get; set; }

        public bool IsSelected { get; set; }

        public string NotificationIdList { get; set; }
    }

    //For Change Password
    public class UserChangePassword
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        [MaxLength(50, ErrorMessage = "Password cannot be more than 50 characters.")] //Added By Maitri Gandhi on 15/4/2016
        //[MaxLength(255, ErrorMessage = "Password cannot contain more than 255 characters.")]
        public string CurrentPassword { get; set; }


        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        //[MaxLength(255, ErrorMessage = "Password cannot contain more than 255 characters.")]
        [MaxLength(50, ErrorMessage = "Password cannot be more than 50 characters.")] //Added By Maitri Gandhi on 15/4/2016
        [MinLength(8, ErrorMessage = "Password should be atleast 8 characters.")]
        [RegularExpression(@"(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[@#$%^&*_+|~!?\.=`:'\(\)\-\\\{\}\[\]\<\>\//]).*$", ErrorMessage = " ")]    //Added By Maitri Gandhi on 8/4/2016 for #2105. Modified for #2131
        public string NewPassword { get; set; }


        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        //[MaxLength(255, ErrorMessage = "Password cannot contain more than 255 characters.")]
        [MaxLength(50, ErrorMessage = "Password cannot be more than 50 characters.")] //Added By Maitri Gandhi on 15/4/2016
        public string ConfirmNewPassword { get; set; }
    }
    
    public class SearchEntity
    {
        public string category { get; set; }
        public string label { get; set; }
        public int value { get; set; }
    }

    public class AlertRule
    {
        public SelectList GoalType { get; set; }
        public SelectList GoalNum { get; set; }
        public SelectList PerformanceComparison { get; set; }
        public SelectList lstFrequency { get; set; }
        public SelectList lstWeekdays { get; set; }
        public List<AlertRuleDetail> lstAlertRule { get; set; }
    }
    public class AlertRuleDetail
    {
        public string EntityID { get; set; }
        public string EntityType { get; set; }
        public string Indicator { get; set; }
        public string IndicatorComparision { get; set; }
        public string IndicatorGoal { get; set; }
        public string CompletionGoal { get; set; }
        public string Frequency { get; set; }
        public string DayOfWeek { get; set; }
        public string DateOfMonth { get; set; }
        public string RuleSummary { get; set; }
        public string EntityName {get;set;}
        public int RuleId{get;set;}
        public bool IsDisable { get; set; }
    }
    public class AlertSummary
    {
        public string Description { get; set; }
        public string AlertCreatedDate { get; set; }
        public int AlertId { get; set; }
    }
    public class NotificationSummary
    {
        public string Description { get; set; }
        public string NotificationCreatedDate { get; set; }
        public int NotificationId { get; set; }
        public string ActionName { get; set; }
        public string PlanTitle { get; set; }
        public int? RequestCount { get; set; }
        public int? ComponentId { get; set; }
    }
    public class UserAlertsNotification
    {
        public int? NotificationId { get; set; }
        public int? AlertId { get; set; }
        public string Description { get; set; }
        public string CreatedDate { get; set; }
        public string ActionName { get; set; }
        public int? PlanID { get; set; }
    }


}
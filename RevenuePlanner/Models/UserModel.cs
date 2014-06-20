using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class UserModel
    {
        [Key]
        public Guid UserId { get; set; }

        [RegularExpression("^[^<>~%^;/|]+", ErrorMessage = "^<>~%;/| characters are not allowed in First Name.")]
        [Display(Name = "First Name")]
        [MaxLength(50, ErrorMessage = "Maximum 50 characters are allowed in First Name.")]
        [Required]
        public string FirstName { get; set; }

        [RegularExpression("^[^<>~%^;/|]+", ErrorMessage = "^<>~%;/| characters are not allowed in Last Name.")]
        [Display(Name = "Last Name")]
        [MaxLength(50, ErrorMessage = "Maximum 50 characters are allowed in Last Name.")]
        [Required]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Email")]
        //[RegularExpression("^[A-Za-z0-9_\\+-]+(\\.[A-Za-z0-9_\\+-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9-]+)*\\.([A-Za-z]{2,4})$", ErrorMessage = "Not a valid Email.")]
        [MaxLength(255, ErrorMessage = "Email cannot contain more than 255 characters.")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [MaxLength(255, ErrorMessage = "Password cannot contain more than 255 characters.")]
        [MinLength(8, ErrorMessage = "Password should be atleast 8 characters.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [MaxLength(255, ErrorMessage = "Confirm Password cannot contain more than 255 characters.")]
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
        [RegularExpression("^[^<>~%^;/|]+", ErrorMessage = "^<>~%;/| characters are not allowed in Display Name.")]
        [MaxLength(255, ErrorMessage = "Display Name cannot contain more than 255 characters.")]
        public string DisplayName { get; set; }

        [Required]
        [Display(Name = "Job Title")]
        [RegularExpression("^[^<>~%^;/|]+", ErrorMessage = "^<>~%;/| characters are not allowed in Job Title.")]
        [MaxLength(50, ErrorMessage = "Job Title cannot contain more than 50 characters.")]
        public string JobTitle { get; set; }

        [Display(Name = "Business Unit")]
        public Guid BusinessUnitId { get; set; }

        [Display(Name = "Business Unit")]
        public string BusinessUnit { get; set; }

        [Display(Name = "Region")]
        public Guid GeographyId { get; set; }

        [Display(Name = "Region")]
        public string Geography { get; set; }

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
        [RegularExpression("^[0-9]{3}-[0-9]{3}-[0-9]{4}|^[0-9]{10}", ErrorMessage = "Not a valid phone number.")]
        [MaxLength(12, ErrorMessage = "Phone cannot contain more than 12 numbers.")]
        public string Phone { get; set; }
        // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
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
        [MaxLength(255, ErrorMessage = "Password cannot contain more than 255 characters.")]
        public string CurrentPassword { get; set; }


        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [MaxLength(255, ErrorMessage = "Password cannot contain more than 255 characters.")]
        [MinLength(8, ErrorMessage = "Password should be atleast 8 characters.")]
        public string NewPassword { get; set; }


        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [MaxLength(255, ErrorMessage = "Password cannot contain more than 255 characters.")]
        public string ConfirmNewPassword { get; set; }
    }
}
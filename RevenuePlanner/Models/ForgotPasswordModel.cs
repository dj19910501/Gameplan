using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Models
{
    // For Forgot Password
    public class ForgotPasswordModel
    {
        [Display(Name = "Email")]
        [Required]
        //[RegularExpression("^[A-Za-z0-9_\\+-]+(\\.[A-Za-z0-9_\\+-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9-]+)*\\.([A-Za-z]{2,4})$", ErrorMessage = "Not a valid email")]
        [MaxLength(100, ErrorMessage = "Email cannot be more than 100 characters.")]
        public string UserEmail { get; set; }

        public bool IsSuccess { get; set; }

    }

    // For Security Question
    public class SecurityQuestionModel
    {
        public Guid PasswordResetRequestId { get; set; }

        public int UserId { get; set; }

        public int AttemptCount { get; set; }

        public string SecurityQuestion { get; set; }

        [Required]
        [MaxLength(255, ErrorMessage = "Answer cannot be more than 255 characters.")]
        public string Answer { get; set; }
    }

    //For Reset Password
    public class ResetPasswordModel
    {
        //Password RequestId 
        public Guid RequestId { get; set; }

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

        public bool IsSuccess { get; set; }
    }

    // For Security Question list
    public class SecurityQuestionListModel
    {
        [Display(Name = "Security Question")]
        public IEnumerable<SelectListItem> SecurityQuestionList { get; set; }

        public int SecurityQuestionId { get; set; }

        [AllowHtml]
        [Display(Name = "Answer")]
        public string Answer { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace RevenuePlanner.Models
{
    public class LoginModel
    {
        [Display(Name = "Email")]
        [Required]
        [RegularExpression("^[A-Za-z0-9_\\+-]+(\\.[A-Za-z0-9_\\+-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9-]+)*\\.([A-Za-z]{2,4})$", ErrorMessage = "Not a valid email")]
        [MaxLength(100, ErrorMessage = "Email cannot be more than 100 characters.")]
        public string UserEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        //[RegularExpression("^[^<>~%^;/|]+", ErrorMessage = "^<>~%;/| characters are not allowed in password")]
        [MaxLength(50, ErrorMessage = "Password cannot be more than 50 characters.")]
        //[MinLength(8, ErrorMessage = "Password should be atleast 8 characters.")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class MVCUrl
    {
        public string actionName { get; set; }
        public string controllerName { get; set; }
        public string queryString { get; set; }
    }
}
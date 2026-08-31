using System.ComponentModel.DataAnnotations;
namespace TechNova.Models
{
    public class RegisterInvestor
    {

        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company name is required.")]
        [Display(Name = "Company / Organization")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{8,}$",
            ErrorMessage = "Password must contain at least 8 characters, including an uppercase letter, lowercase letter, number, and special character.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please give your investment preferences.")]
        [Display(Name = "Investment Preference")]
        public string? Preference { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Enter a valid amount.")]
        [Display(Name = "Investment Range")]
        public decimal? InvestmentRange { get; set; }
    }
}

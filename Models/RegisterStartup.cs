using System.ComponentModel.DataAnnotations;
namespace TechNova.Models
{
    public class RegisterStartup
    {
        [Required(ErrorMessage = "Company name is required.")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please describe your startup.")]
        public string Description { get; set; } = string.Empty;

        [Url(ErrorMessage = "Enter a valid website URL.")]
        public string? Website { get; set; }

        [Required(ErrorMessage = "Funding requirement is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Enter a valid funding amount.")]
        [Display(Name = "Funding Required")]
        public decimal FundingRequired { get; set; }

        [Required(ErrorMessage = "Please select your business stage.")]
        [Display(Name = "Business Stage")]
        public string BusinessStage { get; set; } = string.Empty;
    }
}

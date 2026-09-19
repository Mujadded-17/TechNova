using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TechNova.Models
{
    public class Investor
    {
        public int InvestorID { get; set; }

        [Required(ErrorMessage = "Your name is required.")]
        public string Name { get; set; } = string.Empty;

        public string? CompanyName { get; set; }

        [ValidateNever]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [ValidateNever]
        public string PasswordHash { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public string? ProfileImagePath { get; set; }

        public string? Preference { get; set; }

        [Range(0, 1_000_000_000)]
        public decimal? InvestmentRange { get; set; }

        [Range(0, 1_000_000_000)]
        public decimal? MinInvestmentAmount { get; set; }

        [Range(0, 1_000_000_000)]
        public decimal? MaxInvestmentAmount { get; set; }

        public string? InvestorType { get; set; } // Angel, VC, Institution, etc.

        public string? InvestedIndustries { get; set; } // Comma-separated

        public string? Location { get; set; }

        [Url(ErrorMessage = "Enter a valid website URL (including https://).")]
        public string? Website { get; set; }

        public bool ReceiveNotifications { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }


        // ---- Email verification ----

        /// <summary>
        /// Sign-in is refused until this is true. Existing accounts were
        /// grandfathered to true by the AddEmailVerification migration.
        /// </summary>
        public bool EmailVerified { get; set; } = false;

        /// <summary>Cleared as soon as it is redeemed, so a link works once.</summary>
        public string? EmailVerificationToken { get; set; }

        public DateTime? EmailVerificationSentAt { get; set; }

        [ValidateNever]
        public string VerificationStatus { get; set; } = "Pending";

        public int? VerifiedByAdminID { get; set; }

        // Admin relationship
        public Admin? VerifiedByAdmin { get; set; }

        // Investment requests made by this investor
        public ICollection<InvestmentRequest> InvestmentRequests { get; set; }
            = new List<InvestmentRequest>();

        // Messages involving this investor
        public ICollection<Message> Messages { get; set; }
            = new List<Message>();

        // Favorite startups saved by this investor
        public ICollection<FavoriteStartup> FavoriteStartups { get; set; }
            = new List<FavoriteStartup>();
    }
}
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TechNova.Models
{
    public class Startup
    {
        public int StartupID { get; set; }

        // Validation attributes here are deliberately schema-neutral (no
        // StringLength/MaxLength) so they never create a pending migration.
        [Required(ErrorMessage = "Company name is required.")]
        public string CompanyName { get; set; } = string.Empty;

        [ValidateNever]
        public string Email { get; set; } = string.Empty;

        [ValidateNever]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please describe your startup.")]
        public string Description { get; set; } = string.Empty;

        [Url(ErrorMessage = "Enter a valid website URL (including https://).")]
        public string? Website { get; set; }

        public string? LogoPath { get; set; }

        public string? CoverImagePath { get; set; }

        public string? Tagline { get; set; }

        [Range(1, 1_000_000_000, ErrorMessage = "Enter the amount you are raising.")]
        public decimal FundingRequired { get; set; }

        [Range(0, 1_000_000_000)]
        public decimal AmountRaised { get; set; } = 0;

        [Required(ErrorMessage = "Select your business stage.")]
        public string BusinessStage { get; set; } = string.Empty;

        public string? Industry { get; set; }

        public string? Location { get; set; }

        [Range(1900, 2100, ErrorMessage = "Enter a valid year.")]
        public int? FoundedYear { get; set; }

        public string? ProblemStatement { get; set; }

        public string? Solution { get; set; }

        public string? TargetMarket { get; set; }

        public string? CompetitiveAdvantage { get; set; }

        public string? Traction { get; set; }

        [Range(0, 100, ErrorMessage = "Equity must be between 0 and 100%.")]
        public decimal? EquityOffered { get; set; }

        public DateTime? FundingDeadline { get; set; }

        public string? ContactPhone { get; set; }

        public string? ContactPerson { get; set; }

        [Range(0, 1_000_000)]
        public int? NumberOfEmployees { get; set; }

        public string? BusinessModel { get; set; }

        [Range(0, 1_000_000_000)]
        public decimal? MinimumInvestment { get; set; }

        public bool IsPublished { get; set; } = false;

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

        // Relationships
        public Admin? VerifiedByAdmin { get; set; }

        public ICollection<Founder> Founders { get; set; } = new List<Founder>();

        public ICollection<PitchDeck> PitchDecks { get; set; } = new List<PitchDeck>();

        public ICollection<InvestmentRequest> InvestmentRequests { get; set; } = new List<InvestmentRequest>();

        public ICollection<Message> Messages { get; set; } = new List<Message>();

        public ICollection<FavoriteStartup> FavoredByInvestors { get; set; } = new List<FavoriteStartup>();

        public ICollection<Post> Posts { get; set; } = new List<Post>();

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();

        public ICollection<Video> Videos { get; set; } = new List<Video>();
    }
}
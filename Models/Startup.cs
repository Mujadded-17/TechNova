using System.Diagnostics.Metrics;
using System.ComponentModel.DataAnnotations;

namespace TechNova.Models
{
    public class Startup
    {
        public int StartupID { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Website { get; set; }

        public string? LogoPath { get; set; }

        public string? CoverImagePath { get; set; }

        public string? Tagline { get; set; }

        public decimal FundingRequired { get; set; }

        public decimal AmountRaised { get; set; } = 0;

        public string BusinessStage { get; set; } = string.Empty;

        public string? Industry { get; set; }

        public string? Location { get; set; }

        public int? FoundedYear { get; set; }

        public string? ProblemStatement { get; set; }

        public string? Solution { get; set; }

        public string? TargetMarket { get; set; }

        public string? CompetitiveAdvantage { get; set; }

        public string? Traction { get; set; }

        public decimal? EquityOffered { get; set; }

        public DateTime? FundingDeadline { get; set; }

        public string? ContactPhone { get; set; }

        public string? ContactPerson { get; set; }

        public int? NumberOfEmployees { get; set; }

        public string? BusinessModel { get; set; }

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

        public string VerificationStatus { get; set; } = "Pending";

        public int? VerifiedByAdminID { get; set; }

        // Relationships
        public Admin? VerifiedByAdmin { get; set; }

        public ICollection<Founder> Founders { get; set; } = new List<Founder>();

        public ICollection<PitchDeck> PitchDecks { get; set; } = new List<PitchDeck>();

        public ICollection<InvestmentRequest> InvestmentRequests { get; set; } = new List<InvestmentRequest>();

        public ICollection<Message> Messages { get; set; } = new List<Message>();

        public ICollection<FavoriteStartup> FavoredByInvestors { get; set; } = new List<FavoriteStartup>();
    }
}
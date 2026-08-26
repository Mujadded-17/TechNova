namespace TechNova.Models
{
    public class Investor
    {
        public int InvestorID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? CompanyName { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public string? ProfileImagePath { get; set; }

        public string? Preference { get; set; }

        public decimal? InvestmentRange { get; set; }

        public decimal? MinInvestmentAmount { get; set; }

        public decimal? MaxInvestmentAmount { get; set; }

        public string? InvestorType { get; set; } // Angel, VC, Institution, etc.

        public string? InvestedIndustries { get; set; } // Comma-separated

        public string? Location { get; set; }

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
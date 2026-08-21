using System.Diagnostics.Metrics;
using System.ComponentModel.DataAnnotations;

namespace TechNova.Models
{
    public class Startup
    {
        public int StartupID { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Website { get; set; }

        public string? LogoPath { get; set; }

        public decimal FundingRequired { get; set; }

        public string BusinessStage { get; set; } = string.Empty;

        public string VerificationStatus { get; set; } = "Pending";

        public int? VerifiedByAdminID { get; set; }

        // Relationships
        public Admin? VerifiedByAdmin { get; set; }

        public ICollection<Founder> Founders { get; set; } = new List<Founder>();

        public ICollection<PitchDeck> PitchDecks { get; set; } = new List<PitchDeck>();

        public ICollection<InvestmentRequest> InvestmentRequests { get; set; } = new List<InvestmentRequest>();

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
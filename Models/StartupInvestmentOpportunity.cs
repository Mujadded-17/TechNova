using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TechNova.Models
{
    public class StartupInvestmentOpportunity
    {
        public int OpportunityID { get; set; }

        public int StartupID { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? PitchSummary { get; set; }

        [Range(1, 1_000_000_000)]
        public decimal FundingGoal { get; set; }

        [Range(0, 1_000_000_000)]
        public decimal CurrentFunding { get; set; } = 0;

        public string FundingStage { get; set; } = string.Empty; // Seed, Series A, etc.

        [Range(0, 100)]
        public decimal EquityPercentage { get; set; }

        [Range(1, 1_000_000_000)]
        public decimal MinimumInvestment { get; set; }

        public string? Industry { get; set; }

        public string? Location { get; set; }

        public string BusinessStage { get; set; } = string.Empty;

        public int? FoundedYear { get; set; }

        public int? TeamSize { get; set; }

        public string? Website { get; set; }

        public DateTime? InvestmentDeadline { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string Status { get; set; } = "Active"; // Active, Paused, Closed

        // Relationships. Never posted by a form, so they must be excluded from
        // model validation: a non-nullable navigation is otherwise implicitly
        // [Required] and every CreateOpportunity submit fails silently.
        [ValidateNever]
        public Startup Startup { get; set; } = null!;

        [ValidateNever]
        public ICollection<InvestmentRequest> InvestmentRequests { get; set; } = new List<InvestmentRequest>();
    }
}

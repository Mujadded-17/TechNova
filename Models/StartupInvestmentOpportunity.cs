namespace TechNova.Models
{
    public class StartupInvestmentOpportunity
    {
        public int OpportunityID { get; set; }

        public int StartupID { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? PitchSummary { get; set; }

        public decimal FundingGoal { get; set; }

        public decimal CurrentFunding { get; set; } = 0;

        public string FundingStage { get; set; } = string.Empty; // Seed, Series A, etc.

        public decimal EquityPercentage { get; set; }

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

        // Relationships
        public Startup Startup { get; set; } = null!;

        public ICollection<InvestmentRequest> InvestmentRequests { get; set; } = new List<InvestmentRequest>();
    }
}

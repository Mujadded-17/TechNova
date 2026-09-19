namespace TechNova.Models
{
    /// <summary>Everything the investor dashboard shows, in one typed model.</summary>
    public class InvestorDashboardViewModel
    {
        public Investor Investor { get; set; } = null!;
        public List<Startup> RecentStartups { get; set; } = new();
        public List<InvestmentRequest> MyRequests { get; set; } = new();

        public int ActiveRequestCount { get; set; }
        public int RequestCount { get; set; }
        public int FavoriteCount { get; set; }
        public int UnreadMessages { get; set; }

        /// <summary>True when a trial or paid subscription is active.</summary>
        public bool HasAccess { get; set; }
    }
}

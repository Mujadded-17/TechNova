namespace TechNova.Models
{
    public class FavoriteStartup
    {
        public int FavoriteID { get; set; }

        public int InvestorID { get; set; }

        public int StartupID { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public Investor Investor { get; set; } = null!;

        public Startup Startup { get; set; } = null!;
    }
}

namespace TechNova.Models
{
    public class Admin
    {
        public int AdminID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        // Startups verified by this admin
        public ICollection<Startup> VerifiedStartups { get; set; } = new List<Startup>();

        // Investors verified by this admin
        public ICollection<Investor> VerifiedInvestors { get; set; } = new List<Investor>();
    }
}
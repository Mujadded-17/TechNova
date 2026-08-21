namespace TechNova.Models
{
    public class Founder
    {
        public int FounderID { get; set; }

        public int StartupID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Position { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        // Relationship
        public Startup Startup { get; set; } = null!;
    }
}
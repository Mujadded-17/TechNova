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

        public string? Preference { get; set; }

        public decimal? InvestmentRange { get; set; }

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
    }
}
using System;

namespace TechNova.Models
{
    public class InvestmentRequest
    {
        public int RequestID { get; set; }

        public int InvestorID { get; set; }

        public int StartupID { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public decimal InvestmentAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime? AcceptedDate { get; set; }

        public bool InvestorPdfDownloaded { get; set; } = false;

        public bool StartupPdfDownloaded { get; set; } = false;

        public Investor Investor { get; set; } = null!;

        public Startup Startup { get; set; } = null!;
    }
}
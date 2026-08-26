namespace TechNova.Models
{
    public class Message
    {
        public int MessageID { get; set; }

        public int StartupID { get; set; }

        public int InvestorID { get; set; }

        public string SenderType { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// False until the counterparty opens the thread. Drives the
        /// unread badges in the navbar and conversation list.
        /// </summary>
        public bool IsRead { get; set; } = false;

        // Relationships
        public Startup Startup { get; set; } = null!;

        public Investor Investor { get; set; } = null!;
    }
}
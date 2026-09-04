using System.ComponentModel.DataAnnotations;

namespace TechNova.Models
{
    public class NfcCardRequest
    {
        [Key]
        public int NfcCardRequestID { get; set; }

        // The user requesting the NFC card
        public int UserID { get; set; }

        // "Startup" or "Investor"
        [Required]
        public string UserType { get; set; } = string.Empty;

        // Stripe information
        public string? StripeSessionID { get; set; }

        public string? StripePaymentIntentID { get; set; }

        public decimal Amount { get; set; }

        // Payment status:
        // Pending, Paid, Failed
        [Required]
        public string PaymentStatus { get; set; } = "Pending";

        // Admin/card processing status:
        // PendingPayment, Paid, Approved, Issued, Rejected
        [Required]
        public string RequestStatus { get; set; } = "PendingPayment";

        // Unique URL/token written to the NFC card
        public string? CardToken { get; set; }

        // Physical NFC card identifier, if you want to record it
        public string? CardUID { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? IssuedAt { get; set; }
    }
}
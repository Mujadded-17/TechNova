using System.ComponentModel.DataAnnotations;

namespace TechNova.Models
{
    public static class PaymentStatus
    {
        /// <summary>Recorded, awaiting confirmation from the provider or an admin.</summary>
        public const string Pending = "Pending";

        public const string Paid = "Paid";
        public const string Failed = "Failed";
        public const string Refunded = "Refunded";

        public static readonly string[] All = { Pending, Paid, Failed, Refunded };
    }


    /// <summary>
    /// One attempted or completed charge.
    ///
    /// Deliberately stores no card, account or instrument data — only a
    /// provider name and that provider's own reference. Anything sensitive
    /// stays with the payment processor.
    /// </summary>
    public class Payment
    {
        public int PaymentID { get; set; }

        public int SubscriptionID { get; set; }

        public int InvestorID { get; set; }

        public decimal Amount { get; set; }

        [Required, MaxLength(3)]
        public string Currency { get; set; } = "USD";

        [Required, MaxLength(20)]
        public string Status { get; set; } = PaymentStatus.Pending;

        /// <summary>e.g. "Manual", "Stripe", "SSLCommerz".</summary>
        [Required, MaxLength(40)]
        public string Provider { get; set; } = "Manual";

        /// <summary>
        /// Our own reference, shown to the investor to quote when paying
        /// by transfer, and used by an admin to reconcile.
        /// </summary>
        [Required, MaxLength(40)]
        public string Reference { get; set; } = string.Empty;

        /// <summary>The processor's transaction id, once known.</summary>
        [MaxLength(120)]
        public string? ProviderReference { get; set; }

        /// <summary>Free-text note from the investor (e.g. bKash sender number).</summary>
        [MaxLength(240)]
        public string? PayerNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        /// <summary>Which admin confirmed a manual payment.</summary>
        public int? ConfirmedByAdminID { get; set; }

        // Relationships
        public Subscription Subscription { get; set; } = null!;
        public Investor Investor { get; set; } = null!;
    }
}

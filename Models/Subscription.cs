using System.ComponentModel.DataAnnotations;

namespace TechNova.Models
{
    /// <summary>
    /// Lifecycle of a subscription. An investor has at most one row that
    /// is not Cancelled/Expired at a time.
    /// </summary>
    public static class SubscriptionStatus
    {
        /// <summary>Inside the free trial. Grants full access.</summary>
        public const string Trialing = "Trialing";

        /// <summary>Paid and inside the current period. Grants full access.</summary>
        public const string Active = "Active";

        /// <summary>Awaiting payment confirmation. Does NOT grant access.</summary>
        public const string PendingPayment = "PendingPayment";

        /// <summary>Period ended without renewal. Does NOT grant access.</summary>
        public const string Expired = "Expired";

        /// <summary>Cancelled by the investor or an admin.</summary>
        public const string Cancelled = "Cancelled";

        public static readonly string[] All =
            { Trialing, Active, PendingPayment, Expired, Cancelled };

        /// <summary>The two states that unlock paid features.</summary>
        public static bool GrantsAccess(string status) =>
            status == Trialing || status == Active;
    }


    public class Subscription
    {
        public int SubscriptionID { get; set; }

        public int InvestorID { get; set; }

        public int SubscriptionPlanID { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = SubscriptionStatus.PendingPayment;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Access is granted while this is in the future and the status
        /// allows it. Renewal pushes it forward one month.
        /// </summary>
        public DateTime CurrentPeriodEnd { get; set; }

        public DateTime? TrialEndsAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        /// <summary>
        /// Price captured at subscribe time so later plan price changes
        /// never rewrite what someone already agreed to pay.
        /// </summary>
        public decimal PriceAtSubscription { get; set; }

        [Required, MaxLength(3)]
        public string Currency { get; set; } = "USD";

        public bool AutoRenew { get; set; } = true;

        // Relationships
        public Investor Investor { get; set; } = null!;
        public SubscriptionPlan Plan { get; set; } = null!;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        /// <summary>True when this subscription currently unlocks paid features.</summary>
        public bool IsCurrentlyActive =>
            SubscriptionStatus.GrantsAccess(Status) && CurrentPeriodEnd > DateTime.UtcNow;
    }
}

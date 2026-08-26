using System.ComponentModel.DataAnnotations;

namespace TechNova.Models
{
    /// <summary>
    /// A purchasable monthly plan. Seeded by <c>PlanSeeder</c> and
    /// editable by an administrator, so pricing can change without
    /// a code deploy.
    /// </summary>
    public class SubscriptionPlan
    {
        public int SubscriptionPlanID { get; set; }

        /// <summary>Stable machine key — never shown to users, never reused.</summary>
        [Required, MaxLength(40)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(240)]
        public string? Tagline { get; set; }

        /// <summary>Price per month, in <see cref="Currency"/>.</summary>
        public decimal PriceMonthly { get; set; }

        [Required, MaxLength(3)]
        public string Currency { get; set; } = "USD";

        /// <summary>Days of free trial granted on first subscribe. 0 = none.</summary>
        public int TrialDays { get; set; } = 14;

        /// <summary>Newline-separated bullets rendered on the pricing page.</summary>
        public string Features { get; set; } = string.Empty;

        /// <summary>Null = unlimited. Enforced by the quota checks in SubscriptionService.</summary>
        public int? MonthlyMessageLimit { get; set; }
        public int? MonthlyRequestLimit { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}

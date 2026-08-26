using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;

namespace TechNova.Services
{
    /// <summary>
    /// All subscription business rules live here so controllers, the
    /// authorisation filter and the admin screens agree on one definition
    /// of "has access".
    /// </summary>
    public class SubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// The subscription that currently governs this investor, or null.
        /// Expiry is settled here — a row whose period has elapsed is
        /// written back as Expired so reporting never sees stale Active rows.
        /// </summary>
        public async Task<Subscription?> GetCurrentAsync(int investorId)
        {
            var sub = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.InvestorID == investorId
                            && s.Status != SubscriptionStatus.Cancelled
                            && s.Status != SubscriptionStatus.Expired)
                .OrderByDescending(s => s.SubscriptionID)
                .FirstOrDefaultAsync();

            if (sub == null) return null;

            if (SubscriptionStatus.GrantsAccess(sub.Status)
                && sub.CurrentPeriodEnd <= DateTime.UtcNow)
            {
                sub.Status = SubscriptionStatus.Expired;
                await _context.SaveChangesAsync();
            }

            return sub;
        }


        public async Task<bool> HasAccessAsync(int investorId)
        {
            var sub = await GetCurrentAsync(investorId);
            return sub?.IsCurrentlyActive == true;
        }


        public async Task<List<SubscriptionPlan>> GetPlansAsync() =>
            await _context.SubscriptionPlans
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.PriceMonthly)
                .ToListAsync();


        /// <summary>True when this investor has never had a subscription.</summary>
        public async Task<bool> IsEligibleForTrialAsync(int investorId) =>
            !await _context.Subscriptions.AnyAsync(s => s.InvestorID == investorId);


        /// <summary>
        /// Start a trial. No payment is taken and no Payment row is created.
        /// </summary>
        public async Task<Subscription> StartTrialAsync(int investorId, SubscriptionPlan plan)
        {
            var now = DateTime.UtcNow;
            var trialEnd = now.AddDays(plan.TrialDays);

            var sub = new Subscription
            {
                InvestorID = investorId,
                SubscriptionPlanID = plan.SubscriptionPlanID,
                Status = SubscriptionStatus.Trialing,
                StartedAt = now,
                TrialEndsAt = trialEnd,
                CurrentPeriodEnd = trialEnd,
                PriceAtSubscription = plan.PriceMonthly,
                Currency = plan.Currency,
                AutoRenew = true
            };

            _context.Subscriptions.Add(sub);
            await _context.SaveChangesAsync();

            return sub;
        }


        /// <summary>
        /// Record an intent to subscribe and the charge that must clear.
        ///
        /// No money moves here. The subscription stays PendingPayment and
        /// grants nothing until the payment is confirmed — by an admin for
        /// manual transfers, or by a gateway callback once one is wired in.
        /// </summary>
        public async Task<(Subscription sub, Payment payment)> RequestSubscriptionAsync(
            int investorId,
            SubscriptionPlan plan,
            string provider,
            string? payerNote)
        {
            var existing = await GetCurrentAsync(investorId);

            // Reuse the row if they are re-attempting payment on the same plan.
            var sub = existing is { Status: SubscriptionStatus.PendingPayment }
                      && existing.SubscriptionPlanID == plan.SubscriptionPlanID
                ? existing
                : null;

            if (sub == null)
            {
                sub = new Subscription
                {
                    InvestorID = investorId,
                    SubscriptionPlanID = plan.SubscriptionPlanID,
                    Status = SubscriptionStatus.PendingPayment,
                    StartedAt = DateTime.UtcNow,
                    CurrentPeriodEnd = DateTime.UtcNow,   // no access yet
                    PriceAtSubscription = plan.PriceMonthly,
                    Currency = plan.Currency,
                    AutoRenew = true
                };

                _context.Subscriptions.Add(sub);
                await _context.SaveChangesAsync();
            }

            var payment = new Payment
            {
                SubscriptionID = sub.SubscriptionID,
                InvestorID = investorId,
                Amount = plan.PriceMonthly,
                Currency = plan.Currency,
                Status = PaymentStatus.Pending,
                Provider = provider,
                Reference = await NewReferenceAsync(),
                PayerNote = payerNote,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return (sub, payment);
        }


        /// <summary>
        /// Confirm a pending payment and extend access by one month.
        ///
        /// Idempotent: confirming an already-paid payment does nothing, so
        /// a double-clicked admin button cannot grant two months.
        /// </summary>
        public async Task<bool> ConfirmPaymentAsync(int paymentId, int? adminId)
        {
            var payment = await _context.Payments
                .Include(p => p.Subscription)
                .FirstOrDefaultAsync(p => p.PaymentID == paymentId);

            if (payment == null || payment.Status == PaymentStatus.Paid)
            {
                return false;
            }

            var now = DateTime.UtcNow;

            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = now;
            payment.ConfirmedByAdminID = adminId;

            var sub = payment.Subscription;

            // Extend from whichever is later, so paying early never
            // shortens the time already bought.
            var from = sub.CurrentPeriodEnd > now ? sub.CurrentPeriodEnd : now;

            sub.Status = SubscriptionStatus.Active;
            sub.CurrentPeriodEnd = from.AddMonths(1);

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> MarkPaymentFailedAsync(int paymentId, int? adminId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);

            if (payment == null || payment.Status == PaymentStatus.Paid)
            {
                return false;
            }

            payment.Status = PaymentStatus.Failed;
            payment.ConfirmedByAdminID = adminId;

            await _context.SaveChangesAsync();
            return true;
        }


        /// <summary>
        /// Cancel auto-renewal. Access is deliberately retained until the
        /// end of the period already paid for.
        /// </summary>
        public async Task<bool> CancelAsync(int investorId)
        {
            var sub = await GetCurrentAsync(investorId);
            if (sub == null) return false;

            sub.AutoRenew = false;
            sub.CancelledAt = DateTime.UtcNow;

            if (sub.Status == SubscriptionStatus.PendingPayment)
            {
                sub.Status = SubscriptionStatus.Cancelled;
            }

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<List<Payment>> GetHistoryAsync(int investorId) =>
            await _context.Payments
                .AsNoTracking()
                .Where(p => p.InvestorID == investorId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();


        /// <summary>
        /// Monthly recurring revenue — the summed price of subscriptions
        /// that are Active. Trials are excluded: they are not yet revenue.
        /// </summary>
        public async Task<decimal> GetMrrAsync() =>
            await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active
                            && s.CurrentPeriodEnd > DateTime.UtcNow)
                .SumAsync(s => (decimal?)s.PriceAtSubscription) ?? 0m;


        private async Task<string> NewReferenceAsync()
        {
            // Short, unambiguous, uppercase — quotable over the phone.
            for (var attempt = 0; attempt < 6; attempt++)
            {
                var candidate = "TN-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

                if (!await _context.Payments.AnyAsync(p => p.Reference == candidate))
                {
                    return candidate;
                }
            }

            // Astronomically unlikely; fall back to something guaranteed unique.
            return "TN-" + DateTime.UtcNow.Ticks.ToString()[^10..];
        }
    }
}

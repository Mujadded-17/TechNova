using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;
using TechNova.Services;

namespace TechNova.Controllers
{
    /// <summary>
    /// Administrator module — account management, profile verification,
    /// suspension/removal, and platform reporting.
    ///
    /// Verification state rides on the existing VerificationStatus and
    /// VerifiedByAdminID columns, so this module needs no schema change.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SubscriptionService _subs;

        public AdminController(ApplicationDbContext context, SubscriptionService subs)
        {
            _context = context;
            _subs = subs;
        }

        // Allowed VerificationStatus values.
        private static readonly string[] Statuses =
            { "Pending", "Verified", "Rejected", "Suspended" };

        private int? CurrentAdminId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                ? id
                : null;


        // ============================
        // DASHBOARD
        // ============================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var startups = _context.Startups.AsNoTracking();
            var investors = _context.Investors.AsNoTracking();

            ViewBag.StartupTotal = await startups.CountAsync();
            ViewBag.StartupPending = await startups.CountAsync(s => s.VerificationStatus == "Pending");
            ViewBag.StartupVerified = await startups.CountAsync(s => s.VerificationStatus == "Verified");
            ViewBag.StartupSuspended = await startups.CountAsync(s => s.VerificationStatus == "Suspended");

            ViewBag.InvestorTotal = await investors.CountAsync();
            ViewBag.InvestorPending = await investors.CountAsync(i => i.VerificationStatus == "Pending");
            ViewBag.InvestorVerified = await investors.CountAsync(i => i.VerificationStatus == "Verified");
            ViewBag.InvestorSuspended = await investors.CountAsync(i => i.VerificationStatus == "Suspended");

            ViewBag.RequestTotal = await _context.InvestmentRequests.CountAsync();
            ViewBag.OpportunityTotal = await _context.StartupInvestmentOpportunities.CountAsync();
            ViewBag.MessageTotal = await _context.Messages.CountAsync();

            // Queue of things actually needing an admin decision
            ViewBag.PendingStartups = await startups
                .Where(s => s.VerificationStatus == "Pending")
                .OrderByDescending(s => s.StartupID)
                .Take(5)
                .ToListAsync();

            ViewBag.PendingInvestors = await investors
                .Where(i => i.VerificationStatus == "Pending")
                .OrderByDescending(i => i.InvestorID)
                .Take(5)
                .ToListAsync();

            return View();
        }


        // ============================
        // STARTUP ACCOUNTS
        // ============================

        [HttpGet]
        public async Task<IActionResult> Startups(string? status, string? q)
        {
            var query = _context.Startups.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(s => s.VerificationStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(s =>
                    s.CompanyName.ToLower().Contains(term) ||
                    s.Email.ToLower().Contains(term));
            }

            ViewBag.Status = status ?? "All";
            ViewBag.Query = q;
            ViewBag.Statuses = Statuses;

            return View(await query
                .OrderByDescending(s => s.StartupID)
                .ToListAsync());
        }


        // ============================
        // INVESTOR ACCOUNTS
        // ============================

        [HttpGet]
        public async Task<IActionResult> Investors(string? status, string? q)
        {
            var query = _context.Investors.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(i => i.VerificationStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(i =>
                    i.Name.ToLower().Contains(term) ||
                    i.Email.ToLower().Contains(term));
            }

            ViewBag.Status = status ?? "All";
            ViewBag.Query = q;
            ViewBag.Statuses = Statuses;

            return View(await query
                .OrderByDescending(i => i.InvestorID)
                .ToListAsync());
        }


        // ============================
        // VERIFY / REJECT / SUSPEND / REINSTATE
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStartupStatus(
            int id,
            string status,
            string? returnStatus,
            string? returnQuery)
        {
            if (!Statuses.Contains(status))
            {
                return BadRequest("Unknown status.");
            }

            var startup = await _context.Startups.FindAsync(id);

            if (startup == null)
            {
                return NotFound();
            }

            startup.VerificationStatus = status;

            // Record who made the call; clear it when returning to Pending.
            startup.VerifiedByAdminID = status == "Pending" ? null : CurrentAdminId;

            await _context.SaveChangesAsync();

            TempData["AdminMessage"] =
                $"{startup.CompanyName} is now {status}.";

            return RedirectToAction(nameof(Startups),
                new { status = returnStatus, q = returnQuery });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetInvestorStatus(
            int id,
            string status,
            string? returnStatus,
            string? returnQuery)
        {
            if (!Statuses.Contains(status))
            {
                return BadRequest("Unknown status.");
            }

            var investor = await _context.Investors.FindAsync(id);

            if (investor == null)
            {
                return NotFound();
            }

            investor.VerificationStatus = status;
            investor.VerifiedByAdminID = status == "Pending" ? null : CurrentAdminId;

            await _context.SaveChangesAsync();

            TempData["AdminMessage"] =
                $"{investor.Name} is now {status}.";

            return RedirectToAction(nameof(Investors),
                new { status = returnStatus, q = returnQuery });
        }


        // ============================
        // REMOVE ACCOUNTS
        // Cascades configured in ApplicationDbContext take the dependent
        // rows (founders, pitch decks, requests, messages) with them.
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStartup(int id, string? returnStatus)
        {
            var startup = await _context.Startups.FindAsync(id);

            if (startup == null)
            {
                return NotFound();
            }

            var name = startup.CompanyName;

            _context.Startups.Remove(startup);
            await _context.SaveChangesAsync();

            TempData["AdminMessage"] = $"Removed the startup account “{name}”.";

            return RedirectToAction(nameof(Startups), new { status = returnStatus });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInvestor(int id, string? returnStatus)
        {
            var investor = await _context.Investors.FindAsync(id);

            if (investor == null)
            {
                return NotFound();
            }

            var name = investor.Name;

            _context.Investors.Remove(investor);
            await _context.SaveChangesAsync();

            TempData["AdminMessage"] = $"Removed the investor account “{name}”.";

            return RedirectToAction(nameof(Investors), new { status = returnStatus });
        }


        // ============================
        // REPORTS
        // ============================

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var startups = _context.Startups.AsNoTracking();

            ViewBag.ByStatus = await startups
                .GroupBy(s => s.VerificationStatus)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            ViewBag.ByStage = await startups
                .Where(s => s.BusinessStage != "")
                .GroupBy(s => s.BusinessStage)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            ViewBag.ByIndustry = await startups
                .Where(s => s.Industry != null && s.Industry != "")
                .GroupBy(s => s.Industry!)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            ViewBag.InvestorByStatus = await _context.Investors.AsNoTracking()
                .GroupBy(i => i.VerificationStatus)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            ViewBag.TotalFundingSought = await startups.SumAsync(s => (decimal?)s.FundingRequired) ?? 0m;
            ViewBag.PublishedCount = await startups.CountAsync(s => s.IsPublished);
            ViewBag.StartupCount = await startups.CountAsync();
            ViewBag.InvestorCount = await _context.Investors.CountAsync();

            ViewBag.RequestsByStatus = await _context.InvestmentRequests.AsNoTracking()
                .GroupBy(r => r.Status)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            ViewBag.RequestedTotal = await _context.InvestmentRequests.AsNoTracking()
                .SumAsync(r => (decimal?)r.InvestmentAmount) ?? 0m;

            return View();
        }

        // ============================
        // BILLING — revenue and payment confirmation
        // ============================

        [HttpGet]
        public async Task<IActionResult> Billing(string? status)
        {
            var payments = _context.Payments
                .AsNoTracking()
                .Include(p => p.Investor)
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.Plan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                payments = payments.Where(p => p.Status == status);
            }

            ViewBag.Status = status ?? "Pending";
            ViewBag.Statuses = PaymentStatus.All;

            var subs = _context.Subscriptions.AsNoTracking();
            var now = DateTime.UtcNow;

            ViewBag.Mrr = await _subs.GetMrrAsync();

            ViewBag.ActiveCount = await subs.CountAsync(s =>
                s.Status == SubscriptionStatus.Active && s.CurrentPeriodEnd > now);

            ViewBag.TrialCount = await subs.CountAsync(s =>
                s.Status == SubscriptionStatus.Trialing && s.CurrentPeriodEnd > now);

            ViewBag.PendingCount = await _context.Payments
                .CountAsync(p => p.Status == PaymentStatus.Pending);

            ViewBag.LifetimeRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            ViewBag.Currency = (await _context.SubscriptionPlans
                .Select(p => p.Currency).FirstOrDefaultAsync()) ?? "USD";

            // Default the list to what needs action.
            if (string.IsNullOrWhiteSpace(status))
            {
                payments = payments.Where(p => p.Status == PaymentStatus.Pending);
            }

            return View(await payments
                .OrderByDescending(p => p.CreatedAt)
                .Take(200)
                .ToListAsync());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id, string? returnStatus)
        {
            var ok = await _subs.ConfirmPaymentAsync(id, CurrentAdminId);

            TempData["AdminMessage"] = ok
                ? "Payment confirmed — access granted for one month."
                : "That payment was already settled.";

            return RedirectToAction(nameof(Billing), new { status = returnStatus });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPayment(int id, string? returnStatus)
        {
            var ok = await _subs.MarkPaymentFailedAsync(id, CurrentAdminId);

            TempData["AdminMessage"] = ok
                ? "Payment marked as failed."
                : "That payment is already paid and cannot be failed.";

            return RedirectToAction(nameof(Billing), new { status = returnStatus });
        }
    }
}

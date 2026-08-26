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
    /// Investor-facing billing: pricing, starting a trial, requesting a
    /// subscription, and managing an existing one.
    ///
    /// No card data is accepted or stored anywhere in this controller.
    /// Payment is arranged out of band and confirmed by an administrator,
    /// or by a gateway callback once one is wired in.
    /// </summary>
    [Authorize(Roles = "Investor")]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SubscriptionService _subs;
        private readonly IConfiguration _config;

        public BillingController(
            ApplicationDbContext context,
            SubscriptionService subs,
            IConfiguration config)
        {
            _context = context;
            _subs = subs;
            _config = config;
        }

        private int Me => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


        // ============================
        // PRICING
        // ============================

        [HttpGet]
        public async Task<IActionResult> Plans(string? returnUrl, bool gated = false)
        {
            ViewBag.Current = await _subs.GetCurrentAsync(Me);
            ViewBag.TrialAvailable = await _subs.IsEligibleForTrialAsync(Me);

            // Only keep a local path, so this can't be used as an open redirect.
            ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
            ViewBag.Gated = gated;

            return View(await _subs.GetPlansAsync());
        }


        // ============================
        // START A FREE TRIAL
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartTrial(int planId, string? returnUrl)
        {
            if (!await _subs.IsEligibleForTrialAsync(Me))
            {
                TempData["BillingError"] = "You've already used your free trial.";
                return RedirectToAction(nameof(Plans));
            }

            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.SubscriptionPlanID == planId && p.IsActive);

            if (plan == null) return NotFound();

            var sub = await _subs.StartTrialAsync(Me, plan);

            TempData["BillingMessage"] =
                $"Your {plan.Name} trial is active until {sub.CurrentPeriodEnd.ToLocalTime():dd MMM yyyy}.";

            return Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl!)
                : RedirectToAction(nameof(Manage));
        }


        // ============================
        // SUBSCRIBE (request payment)
        // ============================

        [HttpGet]
        public async Task<IActionResult> Checkout(int planId)
        {
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.SubscriptionPlanID == planId && p.IsActive);

            if (plan == null) return NotFound();

            ViewBag.PayeeName = _config["Billing:PayeeName"] ?? "Tech Nova";
            ViewBag.PayeeAccount = _config["Billing:PayeeAccount"];
            ViewBag.PayeeInstructions = _config["Billing:Instructions"];

            return View(plan);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int planId, string? payerNote)
        {
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.SubscriptionPlanID == planId && p.IsActive);

            if (plan == null) return NotFound();

            var (_, payment) = await _subs.RequestSubscriptionAsync(
                Me, plan, provider: "Manual", payerNote: payerNote);

            TempData["BillingMessage"] =
                $"Reference {payment.Reference} created. Access opens as soon as we confirm your payment.";

            return RedirectToAction(nameof(Manage));
        }


        // ============================
        // MANAGE
        // ============================

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var sub = await _subs.GetCurrentAsync(Me);

            ViewBag.Payments = await _subs.GetHistoryAsync(Me);
            ViewBag.TrialAvailable = await _subs.IsEligibleForTrialAsync(Me);
            ViewBag.PayeeAccount = _config["Billing:PayeeAccount"];

            return View(sub);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel()
        {
            var ok = await _subs.CancelAsync(Me);

            TempData[ok ? "BillingMessage" : "BillingError"] = ok
                ? "Auto-renewal is off. You keep access until the end of the period you've paid for."
                : "You don't have a subscription to cancel.";

            return RedirectToAction(nameof(Manage));
        }
    }
}

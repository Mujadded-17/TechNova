using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using TechNova.Data;
using TechNova.Models;

namespace TechNova.Controllers
{
    [Authorize(Roles = "Investor,Startup")]
    public class NfcCardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NfcCardController> _logger;

        public NfcCardController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<NfcCardController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userType = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userType == "Investor")
            {
                var investor = await _context.Investors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.InvestorID == userId);

                if (investor == null)
                    return NotFound();

                if (investor.VerificationStatus != "Verified")
                    return Forbid();

                var existingRequest = await _context.NfcCardRequests
                    .AsNoTracking()
                    .Where(n =>
                        n.UserID == userId &&
                        n.UserType == "Investor")
                    .OrderByDescending(n => n.RequestedAt)
                    .FirstOrDefaultAsync();

                ViewBag.UserType = "Investor";
                ViewBag.UserName = investor.Name;
                ViewBag.CompanyName = investor.CompanyName;
                ViewBag.ExistingRequest = existingRequest;

                return View();
            }

            if (userType == "Startup")
            {
                var startup = await _context.Startups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.StartupID == userId);

                if (startup == null)
                    return NotFound();

                if (startup.VerificationStatus != "Verified")
                    return Forbid();

                var existingRequest = await _context.NfcCardRequests
                    .AsNoTracking()
                    .Where(n =>
                        n.UserID == userId &&
                        n.UserType == "Startup")
                    .OrderByDescending(n => n.RequestedAt)
                    .FirstOrDefaultAsync();

                ViewBag.UserType = "Startup";
                ViewBag.UserName = startup.CompanyName;
                ViewBag.CompanyName = startup.CompanyName;
                ViewBag.ExistingRequest = existingRequest;

                return View();
            }

            return Forbid();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckout()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userType = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userType != "Investor" && userType != "Startup")
            {
                return Forbid();
            }

            // Check that the user is verified
            if (userType == "Investor")
            {
                var investor = await _context.Investors
                    .FirstOrDefaultAsync(i => i.InvestorID == userId);

                if (investor == null ||
                    investor.VerificationStatus != "Verified")
                {
                    return Forbid();
                }
            }
            else
            {
                var startup = await _context.Startups
                    .FirstOrDefaultAsync(s => s.StartupID == userId);

                if (startup == null ||
                    startup.VerificationStatus != "Verified")
                {
                    return Forbid();
                }
            }

            // Check for an existing request
            var request = await _context.NfcCardRequests
                .Where(n =>
                    n.UserID == userId &&
                    n.UserType == userType)
                .OrderByDescending(n => n.RequestedAt)
                .FirstOrDefaultAsync();

            // If the user already has a paid/approved/issued card,
            // don't allow another request.
            if (request != null &&
                (request.PaymentStatus == "Paid" ||
                 request.RequestStatus == "Approved" ||
                 request.RequestStatus == "Issued"))
            {
                return RedirectToAction("Index");
            }

            // Create a new request if necessary
            if (request == null)
            {
                request = new NfcCardRequest
                {
                    UserID = userId,
                    UserType = userType,
                    Amount = 5.00m,
                    PaymentStatus = "Pending",
                    RequestStatus = "PendingPayment",
                    RequestedAt = DateTime.UtcNow
                };

                _context.NfcCardRequests.Add(request);
                await _context.SaveChangesAsync();
            }

            // Set Stripe test secret key
            StripeConfiguration.ApiKey =
                _configuration["Stripe:SecretKey"];

            if (string.IsNullOrWhiteSpace(StripeConfiguration.ApiKey))
            {
                TempData["Error"] =
                    "Card payments aren't available right now. Please try again later.";
                return RedirectToAction(nameof(Index));
            }

            // Create Stripe Checkout Session
            var options = new SessionCreateOptions
            {
                Mode = "payment",

                SuccessUrl =
                    Url.Action(
                        "PaymentSuccess",
                        "NfcCard",
                        new { session_id = "{CHECKOUT_SESSION_ID}" },
                        Request.Scheme)!,

                CancelUrl =
                    Url.Action(
                        "Index",
                        "NfcCard",
                        null,
                        Request.Scheme)!,

                ClientReferenceId =
                    request.NfcCardRequestID.ToString(),

                Metadata = new Dictionary<string, string>
                {
                    {
                        "NfcCardRequestID",
                        request.NfcCardRequestID.ToString()
                    },
                    {
                        "UserID",
                        userId.ToString()
                    },
                    {
                        "UserType",
                        userType
                    }
                },

                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",

                            UnitAmount = 500,

                            ProductData =
                                new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "TechNova NFC Business Card",
                                    Description =
                                        "TechNova NFC Business Card"
                                }
                        },

                        Quantity = 1
                    }
                }
            };

            var service = new SessionService();

            Session session;
            try
            {
                session = await service.CreateAsync(options);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe checkout session creation failed for request {Id}.",
                    request.NfcCardRequestID);

                TempData["Error"] =
                    "We couldn't start the payment. Please try again in a moment.";
                return RedirectToAction(nameof(Index));
            }

            // Save Stripe session ID
            request.StripeSessionID = session.Id;

            await _context.SaveChangesAsync();

            // Send user to Stripe Checkout
            return Redirect(session.Url);
        }

        /// <summary>
        /// Stripe sends the customer back here after checkout. The webhook is
        /// the source of truth, but it only fires if it has been pointed at
        /// this deployment — so the session is also checked directly, which
        /// makes the flow complete even when the webhook is not configured.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(string? session_id)
        {
            if (string.IsNullOrWhiteSpace(session_id) ||
                !int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                return RedirectToAction(nameof(Index));
            }

            var request = await _context.NfcCardRequests
                .FirstOrDefaultAsync(n => n.StripeSessionID == session_id && n.UserID == userId);

            if (request == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (request.PaymentStatus != "Paid")
            {
                StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

                try
                {
                    var session = await new SessionService().GetAsync(session_id);

                    if (session.PaymentStatus == "paid")
                    {
                        request.PaymentStatus = "Paid";
                        request.RequestStatus = "PendingAdminApproval";
                        request.PaidAt = DateTime.UtcNow;

                        if (!string.IsNullOrWhiteSpace(session.PaymentIntentId))
                        {
                            request.StripePaymentIntentID = session.PaymentIntentId;
                        }

                        await _context.SaveChangesAsync();
                    }
                }
                catch (StripeException ex)
                {
                    _logger.LogWarning(ex, "Could not verify Stripe session {Session}.", session_id);
                }
            }

            TempData[request.PaymentStatus == "Paid" ? "Success" : "Error"] =
                request.PaymentStatus == "Paid"
                    ? "Payment received. Your card request is now waiting for admin approval."
                    : "We haven't received confirmation of your payment yet. It can take a minute — refresh shortly.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body)
                .ReadToEndAsync();

            var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
            var webhookSecret = _configuration["Stripe:WebhookSecret"];

            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                _logger.LogWarning("Stripe webhook received but Stripe:WebhookSecret is not configured.");
                return BadRequest();
            }

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signatureHeader,
                    webhookSecret
                );

                if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;

                    if (session != null &&
                        session.Metadata.TryGetValue(
                            "NfcCardRequestID",
                            out var requestIdString) &&
                        int.TryParse(requestIdString, out var requestId))
                    {
                        var request = await _context.NfcCardRequests
                            .FirstOrDefaultAsync(
                                n => n.NfcCardRequestID == requestId);

                        if (request != null &&
                            request.PaymentStatus != "Paid")
                        {
                            request.PaymentStatus = "Paid";
                            request.RequestStatus = "PendingAdminApproval";
                            request.PaidAt = DateTime.UtcNow;

                            request.StripeSessionID = session.Id;

                            if (!string.IsNullOrWhiteSpace(session.PaymentIntentId))
                            {
                                request.StripePaymentIntentID =
                                    session.PaymentIntentId;
                            }

                            await _context.SaveChangesAsync();
                        }
                    }
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TechNova.Data;
using TechNova.Filters;
using TechNova.Models;
using TechNova.Services;
using System.Security.Claims;

namespace TechNova.Controllers
{
    [Authorize(Roles = "Investor")]
    public class InvestorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SubscriptionService _subs;

        public InvestorController(ApplicationDbContext context, SubscriptionService subs)
        {
            _context = context;
            _subs = subs;
        }

        // ============================
        // INVESTOR DASHBOARD
        // ============================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
            {
                return RedirectToAction("Login", "Account");
            }

            var investor = await _context.Investors
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvestorID == investorId);

            if (investor == null)
            {
                return NotFound();
            }

            // Get recent startups
            var recentStartups = await _context.Startups
                .AsNoTracking()
                .Where(s => s.IsPublished
                            && s.VerificationStatus != "Rejected"
                            && s.VerificationStatus != "Suspended")
                .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
                .Take(6)
                .ToListAsync();

            // Get investor's requests
            var myRequests = await _context.InvestmentRequests
                .AsNoTracking()
                .Where(r => r.InvestorID == investorId)
                .Include(r => r.Startup)
                .OrderByDescending(r => r.RequestDate)
                .Take(10)
                .ToListAsync();

            var favoriteCount = await _context.FavoriteStartups
                .CountAsync(f => f.InvestorID == investorId);

            var unread = await _context.Messages
                .CountAsync(m => m.InvestorID == investorId && !m.IsRead && m.SenderType == "Startup");

            return View(new InvestorDashboardViewModel
            {
                Investor = investor,
                RecentStartups = recentStartups,
                MyRequests = myRequests,
                ActiveRequestCount = myRequests.Count(r => r.Status == "Pending"),
                RequestCount = await _context.InvestmentRequests.CountAsync(r => r.InvestorID == investorId),
                FavoriteCount = favoriteCount,
                UnreadMessages = unread,
                HasAccess = await _subs.HasAccessAsync(investorId)
            });
        }

        // ============================
        // INVESTOR PROFILE
        // ============================

        // Signed-in only, but not investors only: a startup opens this from
        // its investment-request list, so the controller's Investor role has
        // to be stood down without letting the public in.
        [RequiresSignedIn]
        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            var investor = await _context.Investors
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvestorID == id);

            if (investor == null)
            {
                return NotFound();
            }

            return View(investor);
        }

        // ============================
        // INVESTOR PROFILE EDIT
        // ============================

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
            {
                return RedirectToAction("Login", "Account");
            }

            var investor = await _context.Investors
                .FirstOrDefaultAsync(i => i.InvestorID == investorId);

            if (investor == null)
            {
                return NotFound();
            }

            return View(investor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Investor model)
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId) || model.InvestorID != investorId)
            {
                return Forbid();
            }

            var investor = await _context.Investors.FindAsync(investorId);
            if (investor == null)
            {
                return NotFound();
            }

            if (model.MinInvestmentAmount.HasValue && model.MaxInvestmentAmount.HasValue &&
                model.MinInvestmentAmount > model.MaxInvestmentAmount)
            {
                ModelState.AddModelError(nameof(model.MaxInvestmentAmount),
                    "Maximum investment must be at least the minimum.");
            }

            if (!ModelState.IsValid)
            {
                model.VerificationStatus = investor.VerificationStatus;
                return View(model);
            }

            investor.Name = model.Name;
            investor.Bio = model.Bio;
            investor.CompanyName = model.CompanyName;
            investor.Phone = model.Phone;
            investor.InvestorType = model.InvestorType;
            investor.Location = model.Location;
            investor.Website = model.Website;
            investor.Preference = model.Preference;
            investor.MinInvestmentAmount = model.MinInvestmentAmount;
            investor.MaxInvestmentAmount = model.MaxInvestmentAmount;
            investor.InvestmentRange = model.InvestmentRange;
            investor.InvestedIndustries = model.InvestedIndustries;
            investor.ReceiveNotifications = model.ReceiveNotifications;
            investor.UpdatedAt = DateTime.UtcNow;

            _context.Investors.Update(investor);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Dashboard");
        }

        // ============================
        // DISCOVERY / MARKETPLACE
        // ============================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Discover(
            string? search,
            string? industry,
            string? location,
            string? businessStage,
            decimal? minInvestment,
            decimal? maxFundingGoal,
            int? foundedYear,
            int? minTeamSize,
            decimal? minEquity,
            string? sortBy = "newest")
        {
            var query = _context.Startups
                .AsNoTracking()
                .Where(s => s.IsPublished
                            && s.VerificationStatus != "Rejected"
                            && s.VerificationStatus != "Suspended");

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(s =>
                    s.CompanyName.ToLower().Contains(searchLower) ||
                    s.Description.ToLower().Contains(searchLower) ||
                    s.Tagline!.ToLower().Contains(searchLower));
            }

            if (!string.IsNullOrWhiteSpace(industry))
            {
                query = query.Where(s => s.Industry == industry);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(s => s.Location == location);
            }

            if (!string.IsNullOrWhiteSpace(businessStage))
            {
                query = query.Where(s => s.BusinessStage == businessStage);
            }

            if (minInvestment.HasValue)
            {
                query = query.Where(s => s.MinimumInvestment >= minInvestment);
            }

            if (maxFundingGoal.HasValue)
            {
                query = query.Where(s => s.FundingRequired <= maxFundingGoal);
            }

            if (foundedYear.HasValue)
            {
                query = query.Where(s => s.FoundedYear == foundedYear);
            }

            if (minTeamSize.HasValue)
            {
                query = query.Where(s => s.NumberOfEmployees >= minTeamSize);
            }

            if (minEquity.HasValue)
            {
                query = query.Where(s => s.EquityOffered >= minEquity);
            }

            // Apply sorting
            query = sortBy switch
            {
                "oldest" => query.OrderBy(s => s.CreatedAt),
                "lowest-min-investment" => query.OrderBy(s => s.MinimumInvestment),
                "highest-funding-goal" => query.OrderByDescending(s => s.FundingRequired),
                "most-funded" => query.OrderByDescending(s => s.AmountRaised),
                "recently-updated" => query.OrderByDescending(s => s.UpdatedAt),
                _ => query.OrderByDescending(s => s.CreatedAt) // "newest" default
            };

            var startups = await query.ToListAsync();

            // Which of these the signed-in investor has already saved, so the
            // heart can render in the right state without a request per card.
            ViewBag.SavedIds = new HashSet<int>();
            if (User.IsInRole("Investor") &&
                int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var investorId))
            {
                var ids = startups.Select(s => s.StartupID).ToList();
                ViewBag.SavedIds = (await _context.FavoriteStartups.AsNoTracking()
                    .Where(f => f.InvestorID == investorId && ids.Contains(f.StartupID))
                    .Select(f => f.StartupID)
                    .ToListAsync()).ToHashSet();
            }

            ViewBag.Search = search;
            ViewBag.Industry = industry;
            ViewBag.Location = location;
            ViewBag.BusinessStage = businessStage;
            ViewBag.MinInvestment = minInvestment;
            ViewBag.MaxFundingGoal = maxFundingGoal;
            ViewBag.FoundedYear = foundedYear;
            ViewBag.MinTeamSize = minTeamSize;
            ViewBag.MinEquity = minEquity;
            ViewBag.SortBy = sortBy;

            return View(startups);
        }

        // ============================
        // INVESTMENT REQUESTS
        // ============================

        [HttpGet]
        public async Task<IActionResult> MyRequests()
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = await _context.InvestmentRequests
                .AsNoTracking()
                .Where(r => r.InvestorID == investorId)
                .Include(r => r.Startup)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequiresSubscription]
        public async Task<IActionResult> CreateRequest(int startupId, decimal investmentAmount, string? message)
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Only published startups accept requests — same rule as the directory.
            var startup = await _context.Startups
                .FirstOrDefaultAsync(s => s.StartupID == startupId && s.IsPublished);
            if (startup == null)
            {
                return NotFound();
            }

            // Server-side bounds: the form's min/max are advisory only.
            if (investmentAmount <= 0 || investmentAmount > 1_000_000_000m)
            {
                TempData["Error"] = "Enter an investment amount greater than zero.";
                return RedirectToAction("Startup", "Explore", new { id = startupId });
            }

            if (startup.MinimumInvestment.HasValue && investmentAmount < startup.MinimumInvestment.Value)
            {
                TempData["Error"] =
                    $"{startup.CompanyName} asks for a minimum of ${startup.MinimumInvestment.Value:N0}.";
                return RedirectToAction("Startup", "Explore", new { id = startupId });
            }

            // Check if request already exists
            var existingRequest = await _context.InvestmentRequests
                .FirstOrDefaultAsync(r => r.InvestorID == investorId && r.StartupID == startupId);

            if (existingRequest != null)
            {
                TempData["Error"] = "You have already submitted a request to this startup.";
                return RedirectToAction("Startup", "Explore", new { id = startupId });
            }

            var request = new InvestmentRequest
            {
                InvestorID = investorId,
                StartupID = startupId,
                InvestmentAmount = investmentAmount,
                RequestDate = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.InvestmentRequests.Add(request);

            // There is no note column on the request; the note goes into the
            // conversation instead, where the founders will actually read it.
            message = message?.Trim();
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (message.Length > 1000) message = message[..1000];

                _context.Messages.Add(new Message
                {
                    StartupID = startupId,
                    InvestorID = investorId,
                    SenderType = "Investor",
                    Content = $"Investment request for ${investmentAmount:N0}: {message}",
                    Timestamp = DateTime.UtcNow,
                    IsRead = false
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Your request to invest in {startup.CompanyName} has been sent.";
            return RedirectToAction("MyRequests");
        }

        // ============================
        // FAVORITE STARTUPS
        // ============================

        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
            {
                return RedirectToAction("Login", "Account");
            }

            var favorites = await _context.FavoriteStartups
                .AsNoTracking()
                .Where(f => f.InvestorID == investorId)
                .Include(f => f.Startup)
                .OrderByDescending(f => f.SavedAt)
                .ToListAsync();

            return View(favorites);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Save or unsave a startup. Called two ways: as a fetch() from the
        /// Discover/Favorites cards (returns JSON) and as a plain form post
        /// from the startup page (redirects back via returnUrl).
        /// </summary>
        public async Task<IActionResult> ToggleFavorite(int startupId, string? returnUrl)
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
            {
                return RedirectToAction("Login", "Account");
            }

            var favorite = await _context.FavoriteStartups
                .FirstOrDefaultAsync(f => f.InvestorID == investorId && f.StartupID == startupId);

            string action;

            if (favorite != null)
            {
                _context.FavoriteStartups.Remove(favorite);
                await _context.SaveChangesAsync();
                action = "removed";
            }
            else
            {
                var startup = await _context.Startups.FindAsync(startupId);
                if (startup == null)
                {
                    return NotFound();
                }

                _context.FavoriteStartups.Add(new FavoriteStartup
                {
                    InvestorID = investorId,
                    StartupID = startupId,
                    SavedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                action = "added";
            }

            if (Url.IsLocalUrl(returnUrl))
            {
                TempData["Success"] = action == "added"
                    ? "Saved to your list."
                    : "Removed from your saved startups.";
                return Redirect(returnUrl!);
            }

            return Ok(new { success = true, action });
        }

        // ============================
        // CHECK IF STARTUP IS FAVORITED
        // ============================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> IsFavorited(int startupId)
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
            {
                return Ok(new { isFavorited = false });
            }

            var isFavorited = await _context.FavoriteStartups
                .AnyAsync(f => f.InvestorID == investorId && f.StartupID == startupId);

            return Ok(new { isFavorited });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TechNova.Data;
using TechNova.Models;
using System.Security.Claims;

namespace TechNova.Controllers
{
    [Authorize(Roles = "Investor")]
    public class InvestorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvestorController(ApplicationDbContext context)
        {
            _context = context;
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
                .Where(s => s.IsPublished)
                .OrderByDescending(s => s.UpdatedAt)
                .Take(5)
                .ToListAsync();

            // Get investor's requests
            var myRequests = await _context.InvestmentRequests
                .AsNoTracking()
                .Where(r => r.InvestorID == investorId)
                .Include(r => r.Startup)
                .OrderByDescending(r => r.RequestDate)
                .Take(10)
                .ToListAsync();

            var dashboardData = new
            {
                Investor = investor,
                RecentStartups = recentStartups,
                MyRequests = myRequests
            };

            return View((object)dashboardData);
        }

        // ============================
        // INVESTOR PROFILE
        // ============================

        [AllowAnonymous]
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
            string? fundingStage,
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
                .Where(s => s.IsPublished);

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

            if (!string.IsNullOrWhiteSpace(fundingStage))
            {
                query = query.Where(s => s.BusinessStage == fundingStage);
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

            ViewBag.Search = search;
            ViewBag.Industry = industry;
            ViewBag.Location = location;
            ViewBag.FundingStage = fundingStage;
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
        public async Task<IActionResult> CreateRequest(int startupId, decimal investmentAmount, string? message)
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
            {
                return RedirectToAction("Login", "Account");
            }

            var startup = await _context.Startups.FindAsync(startupId);
            if (startup == null)
            {
                return NotFound();
            }

            // Check if request already exists
            var existingRequest = await _context.InvestmentRequests
                .FirstOrDefaultAsync(r => r.InvestorID == investorId && r.StartupID == startupId);

            if (existingRequest != null)
            {
                TempData["Error"] = "You have already submitted a request to this startup.";
                return RedirectToAction("Discover");
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
            await _context.SaveChangesAsync();

            TempData["Success"] = "Investment request submitted successfully.";
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
        public async Task<IActionResult> ToggleFavorite(int startupId)
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
            {
                return RedirectToAction("Login", "Account");
            }

            var favorite = await _context.FavoriteStartups
                .FirstOrDefaultAsync(f => f.InvestorID == investorId && f.StartupID == startupId);

            if (favorite != null)
            {
                // Remove favorite
                _context.FavoriteStartups.Remove(favorite);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, action = "removed" });
            }
            else
            {
                // Add favorite
                var startup = await _context.Startups.FindAsync(startupId);
                if (startup == null)
                {
                    return NotFound();
                }

                favorite = new FavoriteStartup
                {
                    InvestorID = investorId,
                    StartupID = startupId,
                    SavedAt = DateTime.UtcNow
                };

                _context.FavoriteStartups.Add(favorite);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, action = "added" });
            }
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

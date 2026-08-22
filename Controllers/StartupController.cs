using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TechNova.Data;
using TechNova.Models;
using System.Security.Claims;

namespace TechNova.Controllers
{
    [Authorize(Roles = "Startup")]
    public class StartupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StartupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================
        // STARTUP DASHBOARD
        // ============================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var startup = await _context.Startups
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StartupID == startupId);

            if (startup == null)
            {
                return NotFound();
            }

            return View(startup);
        }

        // ============================
        // STARTUP PROFILE (VIEW)
        // ============================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            var startup = await _context.Startups
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StartupID == id && s.IsPublished);

            if (startup == null)
            {
                return NotFound();
            }

            return View(startup);
        }

        // ============================
        // STARTUP PROFILE EDIT
        // ============================

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var startup = await _context.Startups
                .FirstOrDefaultAsync(s => s.StartupID == startupId);

            if (startup == null)
            {
                return NotFound();
            }

            return View(startup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Startup model)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId) || model.StartupID != startupId)
            {
                return Forbid();
            }

            var startup = await _context.Startups.FindAsync(startupId);
            if (startup == null)
            {
                return NotFound();
            }

            // Update profile fields (don't update email or password here)
            startup.CompanyName = model.CompanyName;
            startup.Description = model.Description;
            startup.Website = model.Website;
            startup.Tagline = model.Tagline;
            startup.FundingRequired = model.FundingRequired;
            startup.AmountRaised = model.AmountRaised;
            startup.BusinessStage = model.BusinessStage;
            startup.Industry = model.Industry;
            startup.Location = model.Location;
            startup.FoundedYear = model.FoundedYear;
            startup.ProblemStatement = model.ProblemStatement;
            startup.Solution = model.Solution;
            startup.TargetMarket = model.TargetMarket;
            startup.CompetitiveAdvantage = model.CompetitiveAdvantage;
            startup.Traction = model.Traction;
            startup.EquityOffered = model.EquityOffered;
            startup.FundingDeadline = model.FundingDeadline;
            startup.ContactPhone = model.ContactPhone;
            startup.ContactPerson = model.ContactPerson;
            startup.NumberOfEmployees = model.NumberOfEmployees;
            startup.BusinessModel = model.BusinessModel;
            startup.MinimumInvestment = model.MinimumInvestment;
            startup.IsPublished = model.IsPublished;
            startup.UpdatedAt = DateTime.UtcNow;

            _context.Startups.Update(startup);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Dashboard");
        }

        // ============================
        // INVESTMENT OPPORTUNITIES
        // ============================

        [HttpGet]
        public async Task<IActionResult> Opportunities()
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var opportunities = await _context.StartupInvestmentOpportunities
                .AsNoTracking()
                .Where(o => o.StartupID == startupId)
                .ToListAsync();

            return View(opportunities);
        }

        [HttpGet]
        public IActionResult CreateOpportunity()
        {
            return View(new StartupInvestmentOpportunity());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOpportunity(StartupInvestmentOpportunity model)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.StartupID = startupId;
            model.CreatedAt = DateTime.UtcNow;

            _context.StartupInvestmentOpportunities.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Investment opportunity created successfully.";
            return RedirectToAction("Opportunities");
        }

        [HttpGet]
        public async Task<IActionResult> EditOpportunity(int id)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var opportunity = await _context.StartupInvestmentOpportunities
                .FirstOrDefaultAsync(o => o.OpportunityID == id && o.StartupID == startupId);

            if (opportunity == null)
            {
                return NotFound();
            }

            return View(opportunity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOpportunity(StartupInvestmentOpportunity model)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var opportunity = await _context.StartupInvestmentOpportunities
                .FirstOrDefaultAsync(o => o.OpportunityID == model.OpportunityID && o.StartupID == startupId);

            if (opportunity == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            opportunity.Title = model.Title;
            opportunity.Description = model.Description;
            opportunity.PitchSummary = model.PitchSummary;
            opportunity.FundingGoal = model.FundingGoal;
            opportunity.CurrentFunding = model.CurrentFunding;
            opportunity.FundingStage = model.FundingStage;
            opportunity.EquityPercentage = model.EquityPercentage;
            opportunity.MinimumInvestment = model.MinimumInvestment;
            opportunity.Industry = model.Industry;
            opportunity.Location = model.Location;
            opportunity.BusinessStage = model.BusinessStage;
            opportunity.FoundedYear = model.FoundedYear;
            opportunity.TeamSize = model.TeamSize;
            opportunity.Website = model.Website;
            opportunity.InvestmentDeadline = model.InvestmentDeadline;
            opportunity.IsPublished = model.IsPublished;
            opportunity.UpdatedAt = DateTime.UtcNow;

            _context.StartupInvestmentOpportunities.Update(opportunity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Investment opportunity updated successfully.";
            return RedirectToAction("Opportunities");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOpportunity(int id)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var opportunity = await _context.StartupInvestmentOpportunities
                .FirstOrDefaultAsync(o => o.OpportunityID == id && o.StartupID == startupId);

            if (opportunity == null)
            {
                return NotFound();
            }

            _context.StartupInvestmentOpportunities.Remove(opportunity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Investment opportunity deleted successfully.";
            return RedirectToAction("Opportunities");
        }

        // ============================
        // INVESTMENT REQUESTS
        // ============================

        [HttpGet]
        public async Task<IActionResult> InvestmentRequests()
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = await _context.InvestmentRequests
                .AsNoTracking()
                .Where(r => r.Startup.StartupID == startupId)
                .Include(r => r.Investor)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRequestStatus(int requestId, string status)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var request = await _context.InvestmentRequests
                .Where(r => r.Startup.StartupID == startupId)
                .FirstOrDefaultAsync(r => r.RequestID == requestId);

            if (request == null)
            {
                return NotFound();
            }

            request.Status = status;
            _context.InvestmentRequests.Update(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Investment request status updated to {status}.";
            return RedirectToAction("InvestmentRequests");
        }
    }
}

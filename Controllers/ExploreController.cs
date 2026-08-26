using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;

namespace TechNova.Controllers
{
    /// <summary>
    /// Public directory of the marketplace.
    ///
    /// Startups are browsable by anyone — that is the acquisition funnel
    /// the landing page feeds. Investor profiles are NOT public: they are
    /// visible to signed-in users only, because an investor listing their
    /// cheque size publicly is an invitation to be spammed.
    /// </summary>
    public class ExploreController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExploreController(ApplicationDbContext context)
        {
            _context = context;
        }

        private const int PageSize = 12;

        /// <summary>
        /// What the public may see: published by the founder AND not
        /// rejected or suspended by an administrator.
        /// </summary>
        private IQueryable<Startup> VisibleStartups =>
            _context.Startups.AsNoTracking()
                .Where(s => s.IsPublished
                            && s.VerificationStatus != "Rejected"
                            && s.VerificationStatus != "Suspended");


        // ============================
        // STARTUP GRID  (public)
        // ============================

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Startups(
            string? q,
            string? industry,
            string? stage,
            string? sort,
            int page = 1)
        {
            var query = VisibleStartups;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(s =>
                    s.CompanyName.ToLower().Contains(term) ||
                    s.Description.ToLower().Contains(term) ||
                    (s.Tagline != null && s.Tagline.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(industry))
            {
                query = query.Where(s => s.Industry == industry);
            }

            if (!string.IsNullOrWhiteSpace(stage))
            {
                query = query.Where(s => s.BusinessStage == stage);
            }

            query = sort switch
            {
                "funding" => query.OrderByDescending(s => s.FundingRequired),
                "name" => query.OrderBy(s => s.CompanyName),
                _ => query.OrderByDescending(s => s.StartupID)
            };

            var total = await query.CountAsync();
            if (page < 1) page = 1;

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageCount = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));

            ViewBag.Query = q;
            ViewBag.Industry = industry;
            ViewBag.Stage = stage;
            ViewBag.Sort = sort;

            // Filter options drawn from what actually exists.
            ViewBag.Industries = await VisibleStartups
                .Where(s => s.Industry != null && s.Industry != "")
                .Select(s => s.Industry!)
                .Distinct().OrderBy(x => x).ToListAsync();

            ViewBag.Stages = await VisibleStartups
                .Where(s => s.BusinessStage != "")
                .Select(s => s.BusinessStage)
                .Distinct().OrderBy(x => x).ToListAsync();

            return View(await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync());
        }


        // ============================
        // STARTUP DETAIL  (public)
        // ============================

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Startup(int id)
        {
            var startup = await VisibleStartups
                .FirstOrDefaultAsync(s => s.StartupID == id);

            if (startup == null) return NotFound();

            ViewBag.Founders = await _context.Founders.AsNoTracking()
                .Where(f => f.StartupID == id)
                .ToListAsync();

            // Only the deck the founder chose to feature.
            ViewBag.Deck = await _context.PitchDecks.AsNoTracking()
                .Where(d => d.StartupID == id && d.IsPrimary)
                .FirstOrDefaultAsync();

            return View(startup);
        }


        // ============================
        // INVESTOR GRID  (signed-in only)
        // ============================

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Investors(string? q, string? preference, int page = 1)
        {
            var query = _context.Investors.AsNoTracking()
                .Where(i => i.VerificationStatus != "Rejected"
                            && i.VerificationStatus != "Suspended");

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(i =>
                    i.Name.ToLower().Contains(term) ||
                    (i.CompanyName != null && i.CompanyName.ToLower().Contains(term)) ||
                    (i.Preference != null && i.Preference.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(preference))
            {
                query = query.Where(i => i.Preference == preference);
            }

            var total = await query.CountAsync();
            if (page < 1) page = 1;

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageCount = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            ViewBag.Query = q;
            ViewBag.Preference = preference;

            ViewBag.Preferences = await _context.Investors.AsNoTracking()
                .Where(i => i.Preference != null && i.Preference != "")
                .Select(i => i.Preference!)
                .Distinct().OrderBy(x => x).ToListAsync();

            return View(await query
                .OrderByDescending(i => i.InvestorID)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync());
        }


        // ============================
        // INVESTOR DETAIL  (signed-in only)
        // ============================

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Investor(int id)
        {
            var investor = await _context.Investors.AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvestorID == id
                                          && i.VerificationStatus != "Rejected"
                                          && i.VerificationStatus != "Suspended");

            if (investor == null) return NotFound();

            return View(investor);
        }
    }
}

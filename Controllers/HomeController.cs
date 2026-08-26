using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TechNova.Data;
using TechNova.Models;

namespace TechNova.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // The discovery section shows real startups. Only ones the
            // founder published and an admin hasn't rejected or suspended.
            ViewBag.Startups = await _context.Startups
                .AsNoTracking()
                .Where(s => s.IsPublished
                            && s.VerificationStatus != "Rejected"
                            && s.VerificationStatus != "Suspended")
                .OrderByDescending(s => s.VerificationStatus == "Verified")
                .ThenByDescending(s => s.StartupID)
                .Take(6)
                .ToListAsync();

            ViewBag.StartupCount = await _context.Startups
                .CountAsync(s => s.IsPublished);

            ViewBag.InvestorCount = await _context.Investors
                .CountAsync(i => i.VerificationStatus == "Verified");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

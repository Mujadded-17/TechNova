using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;

namespace TechNova.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminNfcCardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminNfcCardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var requests = await _context.NfcCardRequests
                .AsNoTracking()
                .OrderByDescending(n => n.RequestedAt)
                .ToListAsync();

            return View(requests);
        }
    }
}
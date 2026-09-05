using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;

namespace TechNova.Controllers
{
    [AllowAnonymous]
    public class CardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/card/{token}")]
        public async Task<IActionResult> Profile(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return NotFound();
            }

            var request = await _context.NfcCardRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(n =>
                    n.CardToken == token &&
                    n.PaymentStatus == "Paid" &&
                    (n.RequestStatus == "Approved" ||
                     n.RequestStatus == "Issued"));

            if (request == null)
            {
                return NotFound();
            }

            if (request.UserType == "Startup")
            {
                var startup = await _context.Startups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.StartupID == request.UserID);

                if (startup == null)
                {
                    return NotFound();
                }

                ViewBag.UserType = "Startup";
                ViewBag.Name = startup.CompanyName;

                return View("Profile");
            }

            if (request.UserType == "Investor")
            {
                var investor = await _context.Investors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i =>
                        i.InvestorID == request.UserID);

                if (investor == null)
                {
                    return NotFound();
                }

                ViewBag.UserType = "Investor";
                ViewBag.Name = investor.Name;
                ViewBag.CompanyName = investor.CompanyName;

                return View("Profile");
            }

            return NotFound();
        }
    }
}
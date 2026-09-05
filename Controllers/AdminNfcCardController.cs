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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.NfcCardRequests
                .FirstOrDefaultAsync(n => n.NfcCardRequestID == id);

            if (request == null)
            {
                return NotFound();
            }

            // Only paid requests can be approved
            if (request.PaymentStatus != "Paid")
            {
                TempData["Error"] =
                    "This NFC card request cannot be approved because payment has not been completed.";

                return RedirectToAction(nameof(Index));
            }

            // Prevent approving an already approved/issued request
            if (request.RequestStatus != "PendingAdminApproval")
            {
                TempData["Error"] =
                    "This NFC card request has already been processed.";

                return RedirectToAction(nameof(Index));
            }

            // Generate a unique token for the NFC card
            request.CardToken = Guid.NewGuid().ToString("N");

            request.ApprovedAt = DateTime.UtcNow;
            request.RequestStatus = "Approved";

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "NFC business card request approved successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
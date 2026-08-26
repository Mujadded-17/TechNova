using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Filters;
using TechNova.Models;
using TechNova.Services;

namespace TechNova.Controllers
{
    /// <summary>
    /// Pitch deck upload (startups) and download (owner + subscribed
    /// investors).
    ///
    /// Uploads never land in wwwroot; downloads are always mediated by
    /// <see cref="Download"/> so authorisation runs before any bytes leave.
    /// </summary>
    [Authorize]
    public class PitchDeckController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PitchDeckStorage _storage;
        private readonly SubscriptionService _subs;

        public PitchDeckController(
            ApplicationDbContext context,
            PitchDeckStorage storage,
            SubscriptionService subs)
        {
            _context = context;
            _storage = storage;
            _subs = subs;
        }

        private int Me => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private const int MaxDecksPerStartup = 5;


        // ============================
        // STARTUP: manage decks
        // ============================

        [HttpGet]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> Index()
        {
            ViewBag.MaxDecks = MaxDecksPerStartup;
            ViewBag.Allowed = PitchDeckStorage.AllowedDescription;

            return View(await _context.PitchDecks
                .AsNoTracking()
                .Where(d => d.StartupID == Me)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Startup")]
        [RequestSizeLimit(PitchDeckStorage.MaxBytes + 1024 * 1024)]
        public async Task<IActionResult> Upload(IFormFile? file, string? title)
        {
            var count = await _context.PitchDecks.CountAsync(d => d.StartupID == Me);

            if (count >= MaxDecksPerStartup)
            {
                TempData["DeckError"] =
                    $"You can keep up to {MaxDecksPerStartup} decks. Remove one first.";
                return RedirectToAction(nameof(Index));
            }

            var check = PitchDeckStorage.Validate(file);

            if (!check.Ok)
            {
                TempData["DeckError"] = check.Error;
                return RedirectToAction(nameof(Index));
            }

            var stored = await _storage.SaveAsync(file!);

            var deck = new PitchDeck
            {
                StartupID = Me,
                FileName = Path.GetFileName(file!.FileName),
                FilePath = stored,
                Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
                ContentType = check.ContentType,
                FileSizeBytes = file.Length,
                UploadDate = DateTime.UtcNow,

                // First deck uploaded becomes the one shown publicly.
                IsPrimary = count == 0
            };

            _context.PitchDecks.Add(deck);
            await _context.SaveChangesAsync();

            TempData["DeckMessage"] = $"Uploaded {deck.FileName}.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> SetPrimary(int id)
        {
            var mine = await _context.PitchDecks
                .Where(d => d.StartupID == Me)
                .ToListAsync();

            var target = mine.FirstOrDefault(d => d.PitchID == id);

            if (target == null) return NotFound();

            foreach (var d in mine) d.IsPrimary = d.PitchID == id;

            await _context.SaveChangesAsync();

            TempData["DeckMessage"] = "Updated the deck shown on your profile.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> Delete(int id)
        {
            // Scoped to the caller — one startup cannot delete another's deck.
            var deck = await _context.PitchDecks
                .FirstOrDefaultAsync(d => d.PitchID == id && d.StartupID == Me);

            if (deck == null) return NotFound();

            _storage.Delete(deck.FilePath);

            var wasPrimary = deck.IsPrimary;

            _context.PitchDecks.Remove(deck);
            await _context.SaveChangesAsync();

            // Promote another deck so the profile is never left without one.
            if (wasPrimary)
            {
                var next = await _context.PitchDecks
                    .Where(d => d.StartupID == Me)
                    .OrderByDescending(d => d.UploadDate)
                    .FirstOrDefaultAsync();

                if (next != null)
                {
                    next.IsPrimary = true;
                    await _context.SaveChangesAsync();
                }
            }

            TempData["DeckMessage"] = "Deck removed.";
            return RedirectToAction(nameof(Index));
        }


        // ============================
        // DOWNLOAD
        // Owner always; investors need an active subscription.
        // ============================

        [HttpGet]
        [RequiresSubscription]
        public async Task<IActionResult> Download(int id)
        {
            var deck = await _context.PitchDecks
                .AsNoTracking()
                .Include(d => d.Startup)
                .FirstOrDefaultAsync(d => d.PitchID == id);

            if (deck == null) return NotFound();

            var isOwner = User.IsInRole("Startup") && deck.StartupID == Me;
            var isInvestor = User.IsInRole("Investor");
            var isAdmin = User.IsInRole("Admin");

            if (!isOwner && !isInvestor && !isAdmin)
            {
                return Forbid();
            }

            // Investors only see decks the startup actually published.
            if (isInvestor && !deck.Startup.IsPublished)
            {
                return NotFound();
            }

            var path = _storage.ResolvePath(deck.FilePath);

            if (path == null)
            {
                return NotFound();
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(path);
            var name = PitchDeckStorage.SafeDownloadName(deck);

            // PDFs open in the browser's viewer; Office files download.
            if (deck.ContentType == "application/pdf")
            {
                Response.Headers.ContentDisposition = $"inline; filename=\"{name}\"";
                return File(bytes, deck.ContentType);
            }

            return File(bytes, deck.ContentType, name);
        }
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;

namespace TechNova.Controllers
{
    /// <summary>
    /// Direct messaging between a startup and an investor.
    ///
    /// A conversation is identified by the (StartupID, InvestorID) pair —
    /// there is no Conversation table, so the pair is the thread key.
    /// Every action resolves the caller's own id from their claims and
    /// scopes the query to it, so one party can never read or write into
    /// a thread they are not part of.
    /// </summary>
    [Authorize(Roles = "Startup,Investor")]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        private const int MaxLength = 4000;

        private bool IsStartup => User.IsInRole("Startup");

        private int CurrentId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


        // ============================
        // CONVERSATION LIST
        // ============================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var me = CurrentId;

            // Only threads this account is a participant in.
            var mine = IsStartup
                ? _context.Messages.Where(m => m.StartupID == me)
                : _context.Messages.Where(m => m.InvestorID == me);

            var threads = await mine
                .AsNoTracking()
                .GroupBy(m => new { m.StartupID, m.InvestorID })
                .Select(g => new ConversationSummary
                {
                    StartupID = g.Key.StartupID,
                    InvestorID = g.Key.InvestorID,
                    LastMessage = g.OrderByDescending(m => m.Timestamp)
                                   .Select(m => m.Content).First(),
                    LastAt = g.Max(m => m.Timestamp),

                    // Unread = sent by the other side and not yet opened
                    Unread = g.Count(m => !m.IsRead &&
                        m.SenderType != (IsStartup ? "Startup" : "Investor"))
                })
                .OrderByDescending(t => t.LastAt)
                .ToListAsync();

            // Resolve display names for the counterparties in one round trip.
            if (IsStartup)
            {
                var ids = threads.Select(t => t.InvestorID).ToList();

                var names = await _context.Investors.AsNoTracking()
                    .Where(i => ids.Contains(i.InvestorID))
                    .ToDictionaryAsync(i => i.InvestorID, i => i.Name);

                foreach (var t in threads)
                {
                    t.CounterpartyId = t.InvestorID;
                    t.CounterpartyName = names.GetValueOrDefault(t.InvestorID, "Unknown investor");
                }
            }
            else
            {
                var ids = threads.Select(t => t.StartupID).ToList();

                var names = await _context.Startups.AsNoTracking()
                    .Where(s => ids.Contains(s.StartupID))
                    .ToDictionaryAsync(s => s.StartupID, s => s.CompanyName);

                foreach (var t in threads)
                {
                    t.CounterpartyId = t.StartupID;
                    t.CounterpartyName = names.GetValueOrDefault(t.StartupID, "Unknown startup");
                }
            }

            ViewBag.IsStartup = IsStartup;

            return View(threads);
        }


        // ============================
        // ONE THREAD
        // id = the counterparty's id
        // ============================

        [HttpGet]
        public async Task<IActionResult> Thread(int id)
        {
            var me = CurrentId;

            var startupId = IsStartup ? me : id;
            var investorId = IsStartup ? id : me;

            // The counterparty must actually exist before opening a thread.
            var startup = await _context.Startups.AsNoTracking()
                .FirstOrDefaultAsync(s => s.StartupID == startupId);

            var investor = await _context.Investors.AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvestorID == investorId);

            if (startup == null || investor == null)
            {
                return NotFound();
            }

            var messages = await _context.Messages
                .Where(m => m.StartupID == startupId && m.InvestorID == investorId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            // Opening the thread clears the other side's unread messages.
            var incoming = IsStartup ? "Investor" : "Startup";

            var unread = messages
                .Where(m => !m.IsRead && m.SenderType == incoming)
                .ToList();

            if (unread.Count > 0)
            {
                foreach (var m in unread) m.IsRead = true;
                await _context.SaveChangesAsync();
            }

            ViewBag.IsStartup = IsStartup;
            ViewBag.CounterpartyId = id;
            ViewBag.CounterpartyName = IsStartup ? investor.Name : startup.CompanyName;
            ViewBag.CounterpartySub = IsStartup
                ? (string.IsNullOrWhiteSpace(investor.CompanyName) ? "Investor" : investor.CompanyName!)
                : (string.IsNullOrWhiteSpace(startup.Industry) ? "Startup" : startup.Industry!);

            return View(messages);
        }


        // ============================
        // SEND
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int id, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["MessageError"] = "Write something before sending.";
                return RedirectToAction(nameof(Thread), new { id });
            }

            content = content.Trim();

            if (content.Length > MaxLength)
            {
                content = content[..MaxLength];
            }

            var me = CurrentId;

            var startupId = IsStartup ? me : id;
            var investorId = IsStartup ? id : me;

            // Re-check both sides exist so a forged id cannot create
            // an orphan thread against a deleted account.
            var startupOk = await _context.Startups.AnyAsync(s => s.StartupID == startupId);
            var investorOk = await _context.Investors.AnyAsync(i => i.InvestorID == investorId);

            if (!startupOk || !investorOk)
            {
                return NotFound();
            }

            _context.Messages.Add(new Message
            {
                StartupID = startupId,
                InvestorID = investorId,
                SenderType = IsStartup ? "Startup" : "Investor",
                Content = content,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Thread), new { id });
        }


        // ============================
        // START A NEW CONVERSATION
        // Investors reach out first, so this is investor-only.
        // ============================

        [HttpGet]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> New(string? q)
        {
            var me = CurrentId;

            var query = _context.Startups.AsNoTracking()
                .Where(s => s.IsPublished);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(s =>
                    s.CompanyName.ToLower().Contains(term) ||
                    s.Description.ToLower().Contains(term));
            }

            var startups = await query
                .OrderBy(s => s.CompanyName)
                .Take(50)
                .ToListAsync();

            // Mark the ones already in conversation so the UI can say "Open"
            // instead of offering to start a duplicate thread.
            ViewBag.ExistingThreads = await _context.Messages.AsNoTracking()
                .Where(m => m.InvestorID == me)
                .Select(m => m.StartupID)
                .Distinct()
                .ToListAsync();

            ViewBag.Query = q;

            return View(startups);
        }
    }


    /// <summary>Row in the conversation list.</summary>
    public class ConversationSummary
    {
        public int StartupID { get; set; }
        public int InvestorID { get; set; }

        public int CounterpartyId { get; set; }
        public string CounterpartyName { get; set; } = "";

        public string LastMessage { get; set; } = "";
        public DateTime LastAt { get; set; }
        public int Unread { get; set; }
    }
}

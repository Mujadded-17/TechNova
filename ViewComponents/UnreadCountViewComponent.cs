using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;

namespace TechNova.ViewComponents
{
    /// <summary>
    /// Unread-message pip for the navbar.
    ///
    /// One COUNT query per page render for signed-in startups and
    /// investors; returns empty for anyone else so admins and
    /// anonymous visitors cost nothing.
    /// </summary>
    public class UnreadCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public UnreadCountViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = HttpContext.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                return Content(string.Empty);
            }

            var isStartup = user.IsInRole("Startup");
            var isInvestor = user.IsInRole("Investor");

            if (!isStartup && !isInvestor)
            {
                return Content(string.Empty);
            }

            if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var me))
            {
                return Content(string.Empty);
            }

            // Unread means: addressed to me, and sent by the other side.
            var count = isStartup
                ? await _context.Messages.CountAsync(m =>
                      m.StartupID == me && !m.IsRead && m.SenderType == "Investor")
                : await _context.Messages.CountAsync(m =>
                      m.InvestorID == me && !m.IsRead && m.SenderType == "Startup");

            if (count == 0)
            {
                return Content(string.Empty);
            }

            return View("Default", count);
        }
    }
}

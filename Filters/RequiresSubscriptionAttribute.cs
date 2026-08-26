using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TechNova.Services;

namespace TechNova.Filters
{
    /// <summary>
    /// Gates a paid investor feature.
    ///
    /// Put this on any action that should require an active subscription.
    /// Anyone who is not an investor passes straight through, so the same
    /// action can stay usable for startups and admins.
    ///
    ///     [RequiresSubscription]
    ///     public async Task&lt;IActionResult&gt; CreateRequest(...)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiresSubscriptionAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            // Only investors are metered.
            if (!user.IsInRole("Investor"))
            {
                await next();
                return;
            }

            if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var investorId))
            {
                await next();
                return;
            }

            var service = context.HttpContext.RequestServices
                .GetRequiredService<SubscriptionService>();

            if (await service.HasAccessAsync(investorId))
            {
                await next();
                return;
            }

            // Send them to pricing, remembering where they were headed.
            var returnUrl = context.HttpContext.Request.Path
                          + context.HttpContext.Request.QueryString;

            context.Result = new RedirectToActionResult(
                "Plans",
                "Billing",
                new { returnUrl, gated = true });
        }
    }
}

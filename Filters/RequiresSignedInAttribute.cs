using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TechNova.Filters
{
    /// <summary>
    /// Requires a signed-in user of any role.
    ///
    /// This exists because role requirements stack rather than override. An
    /// action inside a controller marked [Authorize(Roles = "Investor")]
    /// cannot widen that to "anyone signed in" by adding a second [Authorize]:
    /// the two are AND-ed and the controller's role still wins. The only way
    /// out is [AllowAnonymous], which switches authorization off altogether —
    /// far more than was wanted, and how an investor profile ended up readable
    /// by anonymous visitors.
    ///
    /// So this attribute carries IAllowAnonymous to drop the inherited policy,
    /// then re-imposes the weaker rule itself:
    ///
    ///     [RequiresSignedIn]
    ///     public async Task&lt;IActionResult&gt; Profile(int id)
    ///
    /// Use it where a page inside a role-scoped controller has to stay
    /// readable by the other side of the marketplace. It is never a way to
    /// make something public — for that, use [AllowAnonymous] and mean it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RequiresSignedInAttribute
        : Attribute, IAllowAnonymous, IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                // Cookie auth turns this into a redirect to LoginPath,
                // carrying returnUrl, so the visitor lands back here.
                context.Result = new ChallengeResult();
            }

            return Task.CompletedTask;
        }
    }
}

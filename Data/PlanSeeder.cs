using Microsoft.EntityFrameworkCore;
using TechNova.Models;

namespace TechNova.Data
{
    /// <summary>
    /// Seeds the starting price list. Runs once — if any plan exists,
    /// this does nothing, so admin edits to pricing are never overwritten.
    /// </summary>
    public static class PlanSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider services,
            IConfiguration configuration,
            ILogger logger)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                logger.LogWarning(
                    "Plan seeding skipped: pending migrations. " +
                    "Run 'dotnet ef database update' and restart.");
                return;
            }

            if (await context.SubscriptionPlans.AnyAsync())
            {
                return;
            }

            var currency = configuration["Billing:Currency"] ?? "USD";

            context.SubscriptionPlans.AddRange(
                new SubscriptionPlan
                {
                    Code = "investor-starter",
                    Name = "Starter",
                    Tagline = "For angels making their first few investments.",
                    PriceMonthly = 29m,
                    Currency = currency,
                    TrialDays = 14,
                    MonthlyMessageLimit = 25,
                    MonthlyRequestLimit = 5,
                    DisplayOrder = 1,
                    Features = string.Join('\n', new[]
                    {
                        "Browse every published startup",
                        "Message up to 25 founders a month",
                        "Send up to 5 investment requests a month",
                        "Save unlimited startups",
                        "Email support"
                    })
                },
                new SubscriptionPlan
                {
                    Code = "investor-pro",
                    Name = "Professional",
                    Tagline = "For active investors building a portfolio.",
                    PriceMonthly = 79m,
                    Currency = currency,
                    TrialDays = 14,
                    MonthlyMessageLimit = null,   // unlimited
                    MonthlyRequestLimit = null,
                    IsFeatured = true,
                    DisplayOrder = 2,
                    Features = string.Join('\n', new[]
                    {
                        "Everything in Starter",
                        "Unlimited messages to founders",
                        "Unlimited investment requests",
                        "Full pitch deck access",
                        "Priority placement in founder inboxes",
                        "Priority support"
                    })
                },
                new SubscriptionPlan
                {
                    Code = "investor-firm",
                    Name = "Firm",
                    Tagline = "For funds and syndicates with a team.",
                    PriceMonthly = 249m,
                    Currency = currency,
                    TrialDays = 14,
                    MonthlyMessageLimit = null,
                    MonthlyRequestLimit = null,
                    DisplayOrder = 3,
                    Features = string.Join('\n', new[]
                    {
                        "Everything in Professional",
                        "Up to 5 team seats",
                        "Deal-flow export",
                        "Dedicated account manager",
                        "Custom reporting"
                    })
                });

            await context.SaveChangesAsync();

            logger.LogInformation(
                "Seeded 3 subscription plans in {Currency}.", currency);
        }
    }
}

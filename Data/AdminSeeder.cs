using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechNova.Models;

namespace TechNova.Data
{
    /// <summary>
    /// Creates a first administrator account.
    ///
    /// The Admins table has always existed and AccountController has always
    /// been able to authenticate against it, but nothing ever inserted a row,
    /// so no admin could sign in. This closes that gap.
    ///
    /// Development only. The credentials come from configuration
    /// (AdminSeed:Email / AdminSeed:Password) and fall back to a documented
    /// default so a fresh clone works without setup. Change the password
    /// before this is exposed anywhere real.
    /// </summary>
    public static class AdminSeeder
    {
        public const string DefaultEmail = "admin@technova.local";
        public const string DefaultPassword = "Admin@12345";

        public static async Task SeedAsync(
            IServiceProvider services,
            IConfiguration configuration,
            ILogger logger)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            // A pending migration would make every query below throw.
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                logger.LogWarning(
                    "Admin seeding skipped: the database has pending migrations. " +
                    "Run 'dotnet ef database update' and restart.");
                return;
            }

            if (await context.Admins.AnyAsync())
            {
                return;     // already seeded — never overwrite an existing admin
            }

            var email = configuration["AdminSeed:Email"] ?? DefaultEmail;
            var password = configuration["AdminSeed:Password"] ?? DefaultPassword;
            var name = configuration["AdminSeed:Name"] ?? "Platform Admin";

            var hasher = new PasswordHasher<object>();

            context.Admins.Add(new Admin
            {
                Name = name,
                Email = email,
                PasswordHash = hasher.HashPassword(new object(), password)
            });

            await context.SaveChangesAsync();

            logger.LogWarning(
                "Seeded the first admin account: {Email} / {Password} — " +
                "change this password before deploying anywhere.",
                email,
                password);
        }
    }
}

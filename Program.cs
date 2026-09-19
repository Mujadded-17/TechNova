using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TechNova.Data;

var builder = WebApplication.CreateBuilder(args);

// Render (and most container hosts) hand the port to bind on via PORT.
// Locally this is unset and the launch profile's URLs are used instead.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No connection string. Set ConnectionStrings__DefaultConnection in the environment.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sql =>
        {
            // A hosted database (or a cold LocalDB) drops connections
            // occasionally. Retry rather than surfacing a 500.
            sql.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
            sql.CommandTimeout(30);
        }));

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddSingleton<TechNova.Services.StoragePaths>();
builder.Services.AddScoped<TechNova.Services.SubscriptionService>();

// Real SMTP when configured; otherwise a development sender that writes
// the message to the sent-emails folder so links are still clickable locally.
if (!string.IsNullOrWhiteSpace(builder.Configuration["Email:Smtp:Host"]))
{
    builder.Services.AddScoped<TechNova.Services.IEmailSender,
                               TechNova.Services.SmtpEmailSender>();
}
else
{
    builder.Services.AddScoped<TechNova.Services.IEmailSender,
                               TechNova.Services.DevEmailSender>();
}

builder.Services.AddScoped<TechNova.Services.EmailVerificationService>();
builder.Services.AddScoped<TechNova.Services.PitchDeckStorage>();
builder.Services.AddScoped<TechNova.Services.IMediaUploadService, TechNova.Services.MediaUploadService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// The platform proxy terminates TLS, so the scheme and client IP only
// survive as X-Forwarded-* headers. Without this the app believes every
// request is plain HTTP and UseHttpsRedirection loops forever.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// MediaUploadService accepts videos up to 100 MB, but Kestrel caps request
// bodies at ~28 MB by default, so those uploads would fail with a 413 before
// reaching any of that validation. Lift both limits to match, with a little
// headroom for multipart overhead.
const long maxUploadBytes = 100L * 1024 * 1024;
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxUploadBytes + (5L * 1024 * 1024);
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxUploadBytes;
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

// Sign-in, registration and resend-verification are the only endpoints an
// anonymous caller can hammer to any effect. A small per-IP fixed window
// stops password guessing without touching normal use.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "text/html; charset=utf-8";
        await context.HttpContext.Response.WriteAsync(
            "<!doctype html><title>Too many attempts</title>" +
            "<p style=\"font-family:sans-serif;padding:40px\">Too many attempts. Please wait a minute and try again.</p>",
            token);
    };
});

var app = builder.Build();

app.UseForwardedHeaders();

// Conservative defaults; nothing here depends on framing or sniffing.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

// Applying migrations on boot suits a single-instance deployment and keeps
// the hosted database in step with the build. Off by default — a multi-
// instance rollout should run them as a separate release step instead.
if (app.Configuration.GetValue("Database:MigrateOnStartup", false))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    app.Logger.LogInformation("Database migrations applied on startup.");
}

// Nothing ever created an administrator, so the admin half of the
// platform was unreachable. Seed one in development, and in any
// environment where a password has been supplied deliberately.
if (app.Environment.IsDevelopment() ||
    !string.IsNullOrWhiteSpace(app.Configuration["AdminSeed:Password"]))
{
    await TechNova.Data.AdminSeeder.SeedAsync(
        app.Services,
        app.Configuration,
        app.Logger);
}

// Subscription plans are reference data the billing pages depend on.
await TechNova.Data.PlanSeeder.SeedAsync(
    app.Services,
    app.Configuration,
    app.Logger);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapStaticAssets();

// Uploaded media lives outside wwwroot in production (a mounted disk), so
// it needs its own file provider. The public URL is identical either way.
var storage = app.Services.GetRequiredService<TechNova.Services.StoragePaths>();
if (storage.UsesExternalRoot)
{
    storage.EnsureCreated(storage.MediaRoot);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(storage.MediaRoot),
        RequestPath = TechNova.Services.StoragePaths.MediaRequestPath
    });
}

app.MapHealthChecks("/healthz").AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

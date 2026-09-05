using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<TechNova.Services.SubscriptionService>();

// Real SMTP when configured; otherwise a development sender that writes
// the message to App_Data/sent-emails so links are still clickable locally.
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
builder.Services.AddScoped<TechNova.Services.IStartupMediaService, TechNova.Services.StartupMediaService>();
builder.Services.AddScoped<TechNova.Services.IMediaUploadService, TechNova.Services.MediaUploadService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

var app = builder.Build();

// Nothing ever created an administrator, so the admin half of the
// platform was unreachable. Seed one in development only.
if (app.Environment.IsDevelopment())
{
    await TechNova.Data.AdminSeeder.SeedAsync(
        app.Services,
        app.Configuration,
        app.Logger);

    await TechNova.Data.PlanSeeder.SeedAsync(
        app.Services,
        app.Configuration,
        app.Logger);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

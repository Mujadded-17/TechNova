using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TechNova.Data;
using TechNova.Models;
using TechNova.Services;

namespace TechNova.Controllers
{
    public class AccountController : Controller, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<object> _passwordHasher;
        private readonly EmailVerificationService _verification;


        private readonly IEmailSender _email;

        [HttpGet]
        public IActionResult RegistrationPending()
        {
            return View();
        }

        public AccountController(
            ApplicationDbContext context,
            IEmailSender email)
        {
            _context = context;
            _email = email;
            _passwordHasher = new PasswordHasher<object>();
        }

        /// <summary>Absolute verify link with a {token} placeholder.</summary>
        private string VerifyUrlTemplate =>
            Url.Action("VerifyEmail", "Account", new { token = "TOKEN_PLACEHOLDER" },
                       Request.Scheme)!
               .Replace("TOKEN_PLACEHOLDER", "{token}");

        // ============================
        // LOGIN
        // ============================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            email = email.Trim();

            // ----------------------------
            // Check Admin
            // ----------------------------

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Email == email);

            if (admin != null &&
                VerifyPassword(admin.PasswordHash, password))
            {
                await SignInUser(
                    admin.AdminID.ToString(),
                    admin.Email,
                    "Admin",
                    admin.Name);

                return RedirectToAction("Dashboard", "Admin");
            }

            // ----------------------------
            // Check Startup
            // ----------------------------

            var startup = await _context.Startups
    .FirstOrDefaultAsync(s => s.Email == email);

            if (startup != null &&
                VerifyPassword(startup.PasswordHash, password))
            {
                if (startup.VerificationStatus == "Pending")
                {
                    ViewBag.Error =
                        "Your account is waiting for admin approval. " +
                        "You will receive an email when your account is approved.";
                    return View();
                }

                if (startup.VerificationStatus == "Rejected")
                {
                    ViewBag.Error =
                        "Your account registration was rejected by the administrator.";
                    return View();
                }

                if (startup.VerificationStatus == "Suspended")
                {
                    ViewBag.Error =
                        "Your account has been suspended. Please contact the administrator.";
                    return View();
                }

                if (startup.VerificationStatus != "Verified")
                {
                    ViewBag.Error =
                        "Your account is not approved for login.";
                    return View();
                }

                await SignInUser(
                    startup.StartupID.ToString(),
                    startup.Email,
                    "Startup",
                    startup.CompanyName);

                return RedirectToAction("Dashboard", "Startup");
            }

            // ----------------------------
            // Check Investor
            // ----------------------------

            var investor = await _context.Investors
    .FirstOrDefaultAsync(i => i.Email == email);

            if (investor != null &&
                VerifyPassword(investor.PasswordHash, password))
            {
                if (investor.VerificationStatus == "Pending")
                {
                    ViewBag.Error =
                        "Your account is waiting for admin approval. " +
                        "You will receive an email when your account is approved.";
                    return View();
                }

                if (investor.VerificationStatus == "Rejected")
                {
                    ViewBag.Error =
                        "Your account registration was rejected by the administrator.";
                    return View();
                }

                if (investor.VerificationStatus == "Suspended")
                {
                    ViewBag.Error =
                        "Your account has been suspended. Please contact the administrator.";
                    return View();
                }

                if (investor.VerificationStatus != "Verified")
                {
                    ViewBag.Error =
                        "Your account is not approved for login.";
                    return View();
                }

                await SignInUser(
                    investor.InvestorID.ToString(),
                    investor.Email,
                    "Investor",
                    investor.Name);

                return RedirectToAction("Dashboard", "Investor");
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        // ============================
        // STARTUP REGISTRATION
        // ============================

        [HttpGet]
        public IActionResult RegisterStartup()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStartup(
            RegisterStartup model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = model.Email.Trim();

            // Check duplicate email
            bool emailExists =
                await _context.Startups.AnyAsync(s => s.Email == email) ||
                await _context.Investors.AnyAsync(i => i.Email == email) ||
                await _context.Admins.AnyAsync(a => a.Email == email);

            if (emailExists)
            {
                ViewBag.Error = "This email is already registered.";
                return View(model);
            }

            // Create Startup object
            var startup = new Startup
            {
                CompanyName = model.CompanyName,
                Email = email,

                PasswordHash = _passwordHasher.HashPassword(
                    new object(),
                    model.Password),

                Description = model.Description,
                Website = model.Website,
                FundingRequired = model.FundingRequired,
                BusinessStage = model.BusinessStage,

                // New startups must be verified by Admin
                VerificationStatus = "Pending"
            };

            _context.Startups.Add(startup);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(RegistrationPending));
        }

        // ============================
        // INVESTOR REGISTRATION
        // ============================

        [HttpGet]
        public IActionResult RegisterInvestor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterInvestor(
            RegisterInvestor model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = model.Email.Trim();

            // Check duplicate email
            bool emailExists =
                await _context.Startups.AnyAsync(s => s.Email == email) ||
                await _context.Investors.AnyAsync(i => i.Email == email) ||
                await _context.Admins.AnyAsync(a => a.Email == email);

            if (emailExists)
            {
                ViewBag.Error = "This email is already registered.";
                return View(model);
            }

            // Create Investor object
            var investor = new Investor
            {
                Name = model.Name,
                CompanyName = model.CompanyName,
                Email = email,
                Phone = model.Phone,

                PasswordHash = _passwordHasher.HashPassword(
                    new object(),
                    model.Password),

                Preference = model.Preference,
                InvestmentRange = model.InvestmentRange,

                // New investors must be verified by Admin
                VerificationStatus = "Pending"
            };

            _context.Investors.Add(investor);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(RegistrationPending));
        }
        // ============================
        // EMAIL VERIFICATION
        // ============================

        /// <summary>Shown right after registering.</summary>
        [HttpGet]
        public IActionResult VerifyEmailSent(string? email)
        {
            ViewBag.Email = email;
            return View();
        }


        /// <summary>Redeem the link from the email.</summary>
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string? token)
        {
            ViewBag.Result = await _verification.VerifyAsync(token);
            return View();
        }


        [HttpGet]
        public IActionResult ResendVerification(string? email)
        {
            ViewBag.Email = email;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(ResendVerification))]
        public async Task<IActionResult> ResendVerificationPost(string email)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                await _verification.ResendAsync(email, VerifyUrlTemplate);
            }

            // Always the same answer, whether or not the address exists —
            // otherwise this endpoint enumerates registered users.
            ViewBag.Sent = true;
            ViewBag.Email = email;

            return View(nameof(ResendVerification));
        }


        // ============================
        // LOGOUT
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }


        // ============================
        // ACCESS DENIED
        // ============================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // ============================
        // PASSWORD VERIFICATION
        // ============================

        private bool VerifyPassword(
            string storedHash,
            string password)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            var result =
                _passwordHasher.VerifyHashedPassword(
                    new object(),
                    storedHash,
                    password);

            return result == PasswordVerificationResult.Success ||
                   result == PasswordVerificationResult.SuccessRehashNeeded;
        }


        // ============================
        // CREATE LOGIN COOKIE
        // ============================--

        private async Task SignInUser(
            string id,
            string email,
            string role,
            string name)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);
        }
    }
}
using System.Net;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TechNova.Data;

namespace TechNova.Services
{
    /// <summary>
    /// Issues, sends and redeems email-verification tokens for both
    /// account types.
    ///
    /// Tokens are 256 bits from a cryptographic RNG, single-use (cleared
    /// on redemption) and expire after <see cref="TokenLifetime"/>.
    /// </summary>
    public class EmailVerificationService
    {
        public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _email;
        private readonly ILogger<EmailVerificationService> _logger;

        public EmailVerificationService(
            ApplicationDbContext context,
            IEmailSender email,
            ILogger<EmailVerificationService> logger)
        {
            _context = context;
            _email = email;
            _logger = logger;
        }


        public static string NewToken()
        {
            // URL-safe base64 — no padding or characters needing escaping.
            var bytes = RandomNumberGenerator.GetBytes(32);

            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }


        /// <summary>Issue a fresh token for a startup and email the link.</summary>
        public async Task SendForStartupAsync(int startupId, string verifyUrlTemplate)
        {
            var startup = await _context.Startups.FindAsync(startupId);
            if (startup == null || startup.EmailVerified) return;

            startup.EmailVerificationToken = NewToken();
            startup.EmailVerificationSentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await SendAsync(
                startup.Email,
                startup.CompanyName,
                verifyUrlTemplate.Replace("{token}", startup.EmailVerificationToken));
        }


        /// <summary>Issue a fresh token for an investor and email the link.</summary>
        public async Task SendForInvestorAsync(int investorId, string verifyUrlTemplate)
        {
            var investor = await _context.Investors.FindAsync(investorId);
            if (investor == null || investor.EmailVerified) return;

            investor.EmailVerificationToken = NewToken();
            investor.EmailVerificationSentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await SendAsync(
                investor.Email,
                investor.Name,
                verifyUrlTemplate.Replace("{token}", investor.EmailVerificationToken));
        }


        /// <summary>
        /// Re-issue for whichever account owns this address.
        ///
        /// Returns nothing about whether the address exists — the caller
        /// always shows the same message, so this cannot be used to
        /// enumerate registered users.
        /// </summary>
        public async Task ResendAsync(string email, string verifyUrlTemplate)
        {
            email = email.Trim();

            var startup = await _context.Startups
                .FirstOrDefaultAsync(s => s.Email == email && !s.EmailVerified);

            if (startup != null)
            {
                await SendForStartupAsync(startup.StartupID, verifyUrlTemplate);
                return;
            }

            var investor = await _context.Investors
                .FirstOrDefaultAsync(i => i.Email == email && !i.EmailVerified);

            if (investor != null)
            {
                await SendForInvestorAsync(investor.InvestorID, verifyUrlTemplate);
            }
        }


        public enum VerifyResult { Success, AlreadyVerified, Invalid, Expired }


        /// <summary>Redeem a token. The token is cleared on success.</summary>
        public async Task<VerifyResult> VerifyAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return VerifyResult.Invalid;
            }

            var startup = await _context.Startups
                .FirstOrDefaultAsync(s => s.EmailVerificationToken == token);

            if (startup != null)
            {
                if (Expired(startup.EmailVerificationSentAt))
                {
                    return VerifyResult.Expired;
                }

                startup.EmailVerified = true;
                startup.EmailVerificationToken = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Startup {Id} verified their email.", startup.StartupID);
                return VerifyResult.Success;
            }

            var investor = await _context.Investors
                .FirstOrDefaultAsync(i => i.EmailVerificationToken == token);

            if (investor != null)
            {
                if (Expired(investor.EmailVerificationSentAt))
                {
                    return VerifyResult.Expired;
                }

                investor.EmailVerified = true;
                investor.EmailVerificationToken = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Investor {Id} verified their email.", investor.InvestorID);
                return VerifyResult.Success;
            }

            // No match: either never valid, or already redeemed and cleared.
            return VerifyResult.Invalid;
        }


        private static bool Expired(DateTime? sentAt) =>
            sentAt == null || DateTime.UtcNow - sentAt > TokenLifetime;


        private async Task SendAsync(string toEmail, string toName, string link)
        {
            var html = $@"
<div style=""font-family:'Inter Tight',Segoe UI,Arial,sans-serif;max-width:520px;margin:0 auto;color:#0a0c0e"">
  <h1 style=""font-size:24px;letter-spacing:-0.04em;margin:0 0 14px"">Confirm your email</h1>

  <p style=""font-size:15px;line-height:1.6;color:#626d78;margin:0 0 22px"">
    Hi {WebUtility.HtmlEncode(toName)}, please confirm this address to activate
    your Tech Nova account.
  </p>

  <p style=""margin:0 0 24px"">
    <a href=""{link}""
       style=""display:inline-block;background:#0f7a5c;color:#fff;text-decoration:none;
              padding:13px 24px;border-radius:999px;font-weight:600;font-size:15px"">
      Confirm email
    </a>
  </p>

  <p style=""font-size:13px;line-height:1.6;color:#8b959e;margin:0 0 8px"">
    Or paste this link into your browser:<br />
    <span style=""word-break:break-all"">{link}</span>
  </p>

  <p style=""font-size:13px;color:#8b959e;margin:18px 0 0"">
    This link expires in 24 hours. If you didn't create a Tech Nova account,
    you can ignore this email.
  </p>
</div>";

            await _email.SendAsync(toEmail, toName, "Confirm your Tech Nova email", html);
        }
    }
}

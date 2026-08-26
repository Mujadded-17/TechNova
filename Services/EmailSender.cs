using System.Net;
using System.Net.Mail;

namespace TechNova.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
    }


    /// <summary>
    /// Real delivery over SMTP. Every value comes from configuration —
    /// no host, account or password is hardcoded. Set these in user
    /// secrets or environment variables, never in appsettings.json:
    ///
    ///   Email:Smtp:Host, :Port, :User, :Password, :UseSsl
    ///   Email:FromAddress, Email:FromName
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var host = _config["Email:Smtp:Host"]!;
            var port = int.TryParse(_config["Email:Smtp:Port"], out var p) ? p : 587;
            var user = _config["Email:Smtp:User"];
            var password = _config["Email:Smtp:Password"];
            var useSsl = !bool.TryParse(_config["Email:Smtp:UseSsl"], out var ssl) || ssl;

            var fromAddress = _config["Email:FromAddress"] ?? user ?? "no-reply@technova.local";
            var fromName = _config["Email:FromName"] ?? "Tech Nova";

            using var client = new SmtpClient(host, port) { EnableSsl = useSsl };

            if (!string.IsNullOrWhiteSpace(user))
            {
                client.Credentials = new NetworkCredential(user, password);
            }

            using var message = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(toEmail, toName));

            try
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("Verification email sent to {Email}.", toEmail);
            }
            catch (Exception ex)
            {
                // Never surface SMTP failures to the visitor — that would
                // reveal whether an address exists. Log and move on.
                _logger.LogError(ex, "Failed sending email to {Email}.", toEmail);
            }
        }
    }


    /// <summary>
    /// Development fallback used when no SMTP host is configured.
    ///
    /// Writes the message to the log and to App_Data/sent-emails so the
    /// verification link can be opened locally without a mail server.
    /// Registered only when SMTP is absent.
    /// </summary>
    public class DevEmailSender : IEmailSender
    {
        private readonly ILogger<DevEmailSender> _logger;
        private readonly IWebHostEnvironment _env;

        public DevEmailSender(ILogger<DevEmailSender> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var dir = Path.Combine(_env.ContentRootPath, "App_Data", "sent-emails");
            Directory.CreateDirectory(dir);

            var file = Path.Combine(
                dir,
                $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Sanitise(toEmail)}.html");

            await File.WriteAllTextAsync(file, htmlBody);

            _logger.LogWarning(
                "SMTP is not configured — email NOT sent. Wrote {Subject} for {Email} to {File}",
                subject, toEmail, file);
        }

        private static string Sanitise(string value) =>
            string.Concat(value.Select(c =>
                char.IsLetterOrDigit(c) || c is '-' or '.' ? c : '_'));
    }
}

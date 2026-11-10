using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using CedarDelivery.Services.Interfaces;

namespace CedarDelivery.Services.Implementations
{
    // MailKit email sender, adapted from CedarPay.
    // Settings come from configuration:
    //   appsettings.json -> Email:SmtpHost, Email:SmtpPort, Email:FromName, Email:FromEmail
    //   user-secrets     -> Email:Username, Email:Password   (never in appsettings.json)
    //   App:BaseUrl      -> used to build absolute links in emails
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public bool IsConfigured => IsEmailConfigured(_config);

        // Shared with Program.cs, which turns on email confirmation only when email works.
        public static bool IsEmailConfigured(IConfiguration config)
        {
            return !string.IsNullOrWhiteSpace(config["Email:SmtpHost"])
                && !string.IsNullOrWhiteSpace(config["Email:Username"])
                && !string.IsNullOrWhiteSpace(config["Email:Password"]);
        }

        public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            if (!IsConfigured)
            {
                // Not an error during development: the app works without email.
                _logger.LogInformation("Email not configured; skipped \"{Subject}\" to {Email}.", subject, toEmail);
                return;
            }

            var username = _config["Email:Username"]!;
            var fromEmail = _config["Email:FromEmail"];
            if (string.IsNullOrWhiteSpace(fromEmail) || fromEmail.StartsWith("YOUR_"))
            {
                fromEmail = username;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_config["Email:FromName"] ?? "CedarDelivery", fromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = WrapInTemplate(htmlBody) }.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = int.TryParse(_config["Email:Timeout"], out var timeout) ? timeout : 30000;

            // STARTTLS (encrypted) by default, as Gmail requires. "Email:UseStartTls": false
            // is only for a local test mail server on this computer.
            var security = bool.TryParse(_config["Email:UseStartTls"], out var useTls) && !useTls
                ? SecureSocketOptions.None
                : SecureSocketOptions.StartTls;

            await client.ConnectAsync(_config["Email:SmtpHost"]!, int.Parse(_config["Email:SmtpPort"] ?? "587"), security);
            await client.AuthenticateAsync(username, _config["Email:Password"]!); // IsConfigured checked both values above
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email \"{Subject}\" sent to {Email}.", subject, toEmail);
        }

        public async Task SendNotificationAsync(string toEmail, string toName, string title, string message, string? link = null, string linkText = "Open CedarDelivery")
        {
            // Titles and messages can contain user text (store names, review comments),
            // so they are HTML-encoded before going into the email.
            var html = $@"
                <h2 style='color:#0b6b4f;margin-top:0'>{WebUtility.HtmlEncode(title)}</h2>
                <p style='font-size:16px;line-height:1.5'>{WebUtility.HtmlEncode(message)}</p>";

            if (!string.IsNullOrEmpty(link))
            {
                html += $@"
                <p style='margin-top:24px'>
                    <a href='{WebUtility.HtmlEncode(AbsoluteUrl(link))}'
                       style='background:#12a87a;color:#fff;padding:12px 24px;border-radius:8px;text-decoration:none;font-weight:600'>
                        {WebUtility.HtmlEncode(linkText)}
                    </a>
                </p>";
            }

            await SendAsync(toEmail, toName, $"CedarDelivery: {title}", html);
        }

        // "/Orders/Details/5" -> "https://localhost:7232/Orders/Details/5"
        private string AbsoluteUrl(string link)
        {
            if (link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return link;
            }
            var baseUrl = (_config["App:BaseUrl"] ?? "https://localhost:7232").TrimEnd('/');
            return baseUrl + link;
        }

        private static string WrapInTemplate(string content)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head><meta charset='utf-8' /></head>
            <body style=""font-family:'Segoe UI',sans-serif;background:#f5f7fa;margin:0;padding:20px"">
                <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden'>
                    <div style='background:linear-gradient(135deg,#0b6b4f,#12a87a);padding:28px 40px;text-align:center'>
                        <div style='font-size:26px;font-weight:800;color:#fff'>🌲 CedarDelivery</div>
                    </div>
                    <div style='padding:32px 40px;color:#1a1a2e'>
                        {content}
                    </div>
                    <div style='padding:18px 40px;text-align:center;font-size:12px;color:#888;border-top:1px solid #f0f0f0'>
                        CedarDelivery — Local stores, delivered fast.
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}

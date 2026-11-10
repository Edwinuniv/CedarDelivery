using Microsoft.AspNetCore.Identity.UI.Services;
using CedarDelivery.Services.Interfaces;

namespace CedarDelivery.Services.Implementations
{
    // Identity's pages (confirm email, forgot password) send emails through IEmailSender.
    // This connects them to our MailKit EmailService, like CedarPay.
    public class IdentityEmailSender : IEmailSender
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<IdentityEmailSender> _logger;

        public IdentityEmailSender(IEmailService emailService, ILogger<IdentityEmailSender> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                // Identity builds the HTML itself (with the confirmation or reset link).
                await _emailService.SendAsync(email, email.Split('@')[0], subject, htmlMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send \"{Subject}\" to {Email}.", subject, email);
                throw;
            }
        }
    }
}

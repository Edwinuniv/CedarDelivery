namespace CedarDelivery.Services.Interfaces
{
    // Sends emails through SMTP (MailKit). Based on CedarPay's EmailService.
    public interface IEmailService
    {
        // False until SMTP settings and credentials are configured (user-secrets).
        bool IsConfigured { get; }

        // htmlBody is inserted into the CedarDelivery email template.
        Task SendAsync(string toEmail, string toName, string subject, string htmlBody);

        // A short message with an optional button, e.g. "View your order".
        // Title and message are plain text (HTML-encoded here).
        Task SendNotificationAsync(string toEmail, string toName, string title, string message, string? link = null, string linkText = "Open CedarDelivery");
    }
}

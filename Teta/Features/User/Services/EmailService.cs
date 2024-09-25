using SendGrid;
using SendGrid.Helpers.Mail;
using TetaBackend.Features.User.Interfaces;

namespace TetaBackend.Features.User.Services;

public class EmailService : IEmailService
{
    private readonly SendGridClient _client;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        var apiKey = configuration.GetSection("SendGrid:ApiKey").Value;
        var fromEmail = configuration.GetSection("SendGrid:FromEmail").Value;
        var fromName = configuration.GetSection("SendGrid:FromName").Value;

        if (apiKey is null || fromEmail is null || fromName is null)
        {
            throw new ArgumentException("Invalid SendGrid configuration.");
        }

        _client = new SendGridClient(apiKey);
        _fromName = fromName;
        _fromEmail = fromEmail;
    }

    public async Task SendEmail(string toEmail, string text)
    {
        var from = new EmailAddress(_fromEmail, _fromName);
        var to = new EmailAddress(toEmail);

        const string subject = "Tetatet App";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, subject, string.Empty);
        var response = await _client.SendEmailAsync(msg);
    }
}
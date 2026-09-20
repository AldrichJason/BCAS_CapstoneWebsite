using System.Net;
using System.Net.Mail;
using BCAS.Api.Service.Interfaces;

namespace BCAS.Api.Service;

/// <summary>
/// Sends mail via SMTP when Smtp:Host is configured. In local development,
/// where no mail server is set up, it logs the message instead so the
/// reset/invite flow can still be exercised end-to-end.
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var host = _configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogInformation(
                "SMTP not configured; logging email instead.\nTo: {To}\nSubject: {Subject}\nBody:\n{Body}",
                toEmail, subject, htmlBody);
            return;
        }

        var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var fromEmail = _configuration["Smtp:FromEmail"] ?? "no-reply@bcas.edu.ph";
        var fromName = _configuration["Smtp:FromName"] ?? "BCAS Admin Portal";

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = string.IsNullOrEmpty(username) ? null : new NetworkCredential(username, password),
        };

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }
}

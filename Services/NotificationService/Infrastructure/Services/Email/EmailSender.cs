using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Settings;

namespace NotificationService.Infrastructure.Services.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        var mail = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = message.Subject,
            IsBodyHtml = message.HtmlBody is not null
        };

        mail.To.Add(new MailAddress(message.To, message.ToName));

        if (message.HtmlBody is not null)
        {
            mail.Body = message.HtmlBody;
            mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                message.PlainBody, null, "text/plain"));
        }
        else
        {
            mail.Body = message.PlainBody;
        }

        await client.SendMailAsync(mail, ct);
        _logger.LogInformation("Email sent to {To} — Subject: {Subject}", message.To, message.Subject);
    }
}
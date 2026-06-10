using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Services;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly INotificationRepository _repo;
    private readonly INotificationPreferenceRepository _prefRepo;
    private readonly IEmailSender _emailSender;
    private readonly IInAppNotificationService _inAppService;
    private readonly ITemplateEngine _templateEngine;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        INotificationRepository repo,
        INotificationPreferenceRepository prefRepo,
        IEmailSender emailSender,
        IInAppNotificationService inAppService,
        ITemplateEngine templateEngine,
        ILogger<NotificationDispatcher> logger)
    {
        _repo = repo;
        _prefRepo = prefRepo;
        _emailSender = emailSender;
        _inAppService = inAppService;
        _templateEngine = templateEngine;
        _logger = logger;
    }

    public async Task DispatchAsync(DispatchNotificationRequest request, CancellationToken ct = default)
    {
        // Load user preferences
        var prefs = await _prefRepo.GetByUserIdAsync(request.RecipientUserId, ct);
        var pref = prefs.FirstOrDefault(p => p.NotificationType == request.Type);

        foreach (var channel in request.Channels)
        {
            // Check user preference — skip if opted out
            if (channel == NotificationChannel.Email && pref is not null && !pref.EmailEnabled)
            {
                _logger.LogDebug("Email notification skipped — user {UserId} opted out of {Type}",
                    request.RecipientUserId, request.Type);
                continue;
            }

            if (channel == NotificationChannel.InApp && pref is not null && !pref.InAppEnabled)
            {
                _logger.LogDebug("InApp notification skipped — user {UserId} opted out of {Type}",
                    request.RecipientUserId, request.Type);
                continue;
            }

            // Render HTML if template provided
            string? htmlBody = null;
            if (channel == NotificationChannel.Email && request.HtmlTemplateName is not null)
            {
                try
                {
                    htmlBody = _templateEngine.Render(request.HtmlTemplateName, request.TemplateData);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Template rendering failed for {Template}", request.HtmlTemplateName);
                }
            }

            // Create notification record
            var notification = Notification.Create(
                request.RecipientUserId,
                request.RecipientEmail,
                request.RecipientName,
                request.Type,
                channel,
                request.Title,
                request.Body,
                request.CorrelationId,
                htmlBody,
                request.ActionUrl,
                request.ReferenceId,
                request.ReferenceType);

            _logger.LogInformation("Dispatching {notification}", notification);

            await _repo.AddAsync(notification, ct);
            await _repo.SaveChangesAsync(ct);

            // Deliver
            try
            {
                if (channel == NotificationChannel.Email)
                {
                    await _emailSender.SendAsync(new EmailMessage
                    {
                        To = request.RecipientEmail,
                        ToName = request.RecipientName,
                        Subject = request.Title,
                        PlainBody = request.Body,
                        HtmlBody = htmlBody
                    }, ct);
                }
                else if (channel == NotificationChannel.InApp)
                {
                    await _inAppService.PushAsync(request.RecipientUserId, new InAppNotificationPayload
                    {
                        NotificationId = notification.Id,
                        Title = request.Title,
                        Body = request.Body,
                        ActionUrl = request.ActionUrl,
                        Type = request.Type.ToString(),
                        CreatedAt = notification.CreatedAt
                    }, ct);
                }

                notification.MarkSent();
                _repo.Update(notification);
                await _repo.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver {Channel} notification {NotificationId}",
                    channel, notification.Id);
                notification.MarkFailed(ex.Message);
                _repo.Update(notification);
                await _repo.SaveChangesAsync(ct);
            }
        }
    }
}
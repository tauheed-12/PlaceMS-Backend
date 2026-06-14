using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class PasswordResetHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.UserPasswordReset;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<PasswordResetHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public PasswordResetHandler(INotificationDispatcher dispatcher, ILogger<PasswordResetHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<UserPasswordResetEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        if (string.IsNullOrEmpty(p.ResetLink))
        {
            _logger.LogWarning("Password reset handler received empty ResetLink for user {UserId} ({Email}). " +
                "Check IdentityService event publisher.", p.UserId, p.Email);
        }

        var templateData = new Dictionary<string, string>
        {
            ["Name"] = p.FullName,
            ["ActionUrl"] = p.ResetLink
        };

        await _dispatcher.DispatchAsync(new DispatchNotificationRequest
        {
            RecipientUserId = p.UserId,
            RecipientEmail = p.Email,
            RecipientName = p.FullName,
            Type = NotificationType.PasswordReset,
            Title = "Reset your password — PMS",
            Body = $"Hi {p.FullName}, please reset your password by clicking the link below:\n{p.ResetLink}",
            HtmlTemplateName = "PasswordReset",
            TemplateData = templateData,
            ActionUrl = p.ResetLink,
            CorrelationId = envelope.CorrelationId,
            Channels = new() { NotificationChannel.Email }  // Email only — not in-app
        }, ct);
    }
}
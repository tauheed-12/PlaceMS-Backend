using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class UserDeactivatedHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.UserDeactivated;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<UserDeactivatedHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public UserDeactivatedHandler(INotificationDispatcher dispatcher, ILogger<UserDeactivatedHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<UserDeactivatedEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        await _dispatcher.DispatchAsync(new DispatchNotificationRequest
        {
            RecipientUserId = p.UserId,
            RecipientEmail = p.Email,
            RecipientName = "User", // We don't have the full name in this event, so we'll just use "User"
            Type = NotificationType.AccountDeactivated,
            Title = "Your account has been deactivated — PMS",
            Body = $"Hi {p.Email}, your account has been deactivated.",
            HtmlTemplateName = "UserDeactivated",
            TemplateData = new Dictionary<string, string>
            {
                ["Email"] = p.Email,
                ["DeactivatedBy"] = p.DeactivatedBy
            },
            ActionUrl = string.Empty, // No action URL for deactivation
            CorrelationId = envelope.CorrelationId,
            Channels = new() { NotificationChannel.Email }  // Email only — not in-app
        }, ct);
    }
}
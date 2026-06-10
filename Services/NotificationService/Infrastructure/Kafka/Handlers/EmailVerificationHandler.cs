using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class EmailVerificationHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.UserEmailVerification;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<EmailVerificationHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public EmailVerificationHandler(INotificationDispatcher dispatcher, ILogger<EmailVerificationHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<UserEmailVerificationEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        await _dispatcher.DispatchAsync(new DispatchNotificationRequest
        {
            RecipientUserId = p.UserId,
            RecipientEmail = p.Email,
            RecipientName = p.FullName,
            Type = NotificationType.EmailVerification,
            Title = "Verify your email — PMS",
            Body = $"Hi {p.FullName}, please verify your email by clicking the link below.",
            HtmlTemplateName = "EmailVerification",
            TemplateData = new Dictionary<string, string>
            {
                ["Name"] = p.FullName,
                ["ActionUrl"] = p.VerificationLink
            },
            ActionUrl = p.VerificationLink,
            CorrelationId = envelope.CorrelationId,
            Channels = new() { NotificationChannel.Email }  // Email only — not in-app
        }, ct);
    }
}
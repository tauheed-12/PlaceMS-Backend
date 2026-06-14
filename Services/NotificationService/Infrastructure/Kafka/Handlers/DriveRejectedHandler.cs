using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class DriveRejectedHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.DriveRejected;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<DriveRejectedHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public DriveRejectedHandler(
        INotificationDispatcher dispatcher,
        ILogger<DriveRejectedHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<DriveRejectedEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        try
        {
            await _dispatcher.DispatchAsync(new DispatchNotificationRequest
            {
                RecipientUserId = p.RecruiterUserId,
                RecipientEmail = p.RecruiterEmail,
                RecipientName = "Recruiter",
                Type = NotificationType.DriveRejected,
                Title = $"Drive rejected — {p.CompanyName}",
                Body = $"Your drive for {p.CompanyName} has been rejected. Reason: {p.RejectionNote}",
                ActionUrl = $"/recruiter/drives/{p.DriveId}",
                ReferenceId = p.DriveId.ToString(),
                ReferenceType = "Drive",
                CorrelationId = envelope.CorrelationId,
                Channels = new() { NotificationChannel.InApp }
            }, ct);

            _logger.LogInformation(
                "Dispatched drive rejected notification for drive {DriveId}",
                p.DriveId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to dispatch drive rejected notification for drive {DriveId}",
                p.DriveId);
        }
    }
}

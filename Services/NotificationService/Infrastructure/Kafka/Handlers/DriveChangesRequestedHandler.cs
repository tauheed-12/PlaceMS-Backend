using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class DriveChangesRequestedHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.DriveChangesRequested;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<DriveChangesRequestedHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public DriveChangesRequestedHandler(
        INotificationDispatcher dispatcher,
        ILogger<DriveChangesRequestedHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<DriveChangesRequestedEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        try
        {
            await _dispatcher.DispatchAsync(new DispatchNotificationRequest
            {
                RecipientUserId = p.RecruiterUserId,
                RecipientEmail = p.RecruiterEmail,
                RecipientName = "Recruiter",
                Type = NotificationType.DriveChangesRequested,
                Title = $"Changes requested — {p.CompanyName}",
                Body = $"The TPO has requested changes to your drive for {p.JobRole}. Please review the feedback and resubmit.",
                ActionUrl = $"/recruiter/drives/{p.DriveId}/edit",
                ReferenceId = p.DriveId.ToString(),
                ReferenceType = "Drive",
                CorrelationId = envelope.CorrelationId,
                Channels = new() { NotificationChannel.InApp }
            }, ct);

            _logger.LogInformation(
                "Dispatched drive changes requested notification for drive {DriveId}",
                p.DriveId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to dispatch drive changes requested notification for drive {DriveId}",
                p.DriveId);
        }
    }
}

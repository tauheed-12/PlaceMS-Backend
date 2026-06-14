using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class DriveResubmittedHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.DriveResubmitted;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<DriveResubmittedHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public DriveResubmittedHandler(
        INotificationDispatcher dispatcher,
        ILogger<DriveResubmittedHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<DriveResubmittedEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        try
        {
            // Notify TPO that drive has been resubmitted
            if (p.TpoUserId != Guid.Empty)
            {
                await _dispatcher.DispatchAsync(new DispatchNotificationRequest
                {
                    RecipientUserId = p.TpoUserId,
                    RecipientEmail = p.TpoEmail,
                    RecipientName = p.TpoName,
                    Type = NotificationType.DriveResubmitted,
                    Title = $"Drive resubmitted — {p.CompanyName}",
                    Body = $"The recruiter has resubmitted the drive for {p.JobRole} after addressing your feedback. Please review and approve/reject.",
                    ActionUrl = $"/tpo/drives/{p.DriveId}/review",
                    ReferenceId = p.DriveId.ToString(),
                    ReferenceType = "Drive",
                    CorrelationId = envelope.CorrelationId,
                    Channels = new() { NotificationChannel.InApp }
                }, ct);

                _logger.LogInformation(
                    "Dispatched drive resubmitted notification to TPO for drive {DriveId}",
                    p.DriveId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to dispatch drive resubmitted notification for drive {DriveId}",
                p.DriveId);
        }
    }
}

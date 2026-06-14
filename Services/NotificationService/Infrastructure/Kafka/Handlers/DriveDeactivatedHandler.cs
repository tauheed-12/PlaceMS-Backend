using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class DriveDeactivatedHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.DriveDeactivated;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<DriveDeactivatedHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public DriveDeactivatedHandler(
        INotificationDispatcher dispatcher,
        ILogger<DriveDeactivatedHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<DriveDeactivatedEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        try
        {
            _logger.LogInformation(
                "Received drive deactivated notification for drive {DriveId} across {CollegeCount} colleges",
                p.DriveId,
                p.CollegeIds.Count);

            // Note: In a production system, you would:
            // 1. Fetch all students who applied to this drive from ApplicationService
            // 2. Fetch recruiter details from IdentityService
            // 3. Dispatch notifications to each student
            // For now, this serves as a placeholder for the notification infrastructure

            _logger.LogInformation(
                "Drive {DriveId} ({CompanyName} - {JobRole}) has been deactivated. Affected students will be notified.",
                p.DriveId,
                p.CompanyName,
                p.JobRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process drive deactivated event for drive {DriveId}",
                p.DriveId);
        }
    }
}

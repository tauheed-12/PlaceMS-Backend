using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class DriveApprovedHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.DriveApproved;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<DriveApprovedHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public DriveApprovedHandler(
        INotificationDispatcher dispatcher,
        ILogger<DriveApprovedHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<DriveApprovedEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        try
        {
            await _dispatcher.DispatchAsync(new DispatchNotificationRequest
            {
                RecipientUserId = Guid.Empty, // Recruiter UserId to be fetched from other data
                RecipientEmail = string.Empty, // Recruiter Email to be fetched from other data
                RecipientName = "Recruiter",
                Type = NotificationType.DriveApproved,
                Title = $"Drive approved — {p.CompanyName}",
                Body = $"Your drive for {p.JobRole} at {p.CompanyName} has been approved by the college.",
                ActionUrl = $"/recruiter/drives/{p.DriveId}",
                ReferenceId = p.DriveId.ToString(),
                ReferenceType = "Drive",
                CorrelationId = envelope.CorrelationId,
                Channels = new() { NotificationChannel.InApp }
            }, ct);

            _logger.LogInformation(
                "Dispatched drive approved notification for drive {DriveId}",
                p.DriveId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to dispatch drive approved notification for drive {DriveId}",
                p.DriveId);
        }
    }
}

using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class DriveApprovalRequestedHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.DriveApprovalRequested;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<DriveApprovalRequestedHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public DriveApprovalRequestedHandler(
        INotificationDispatcher dispatcher,
        ILogger<DriveApprovalRequestedHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<DriveApprovalRequestedEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        await _dispatcher.DispatchAsync(new DispatchNotificationRequest
        {
            RecipientUserId = p.TpoUserId,
            RecipientEmail = p.TpoEmail,
            RecipientName = p.TpoName,
            Type = NotificationType.DriveApprovalRequested,
            Title = $"New drive approval request — {p.CompanyName}",
            Body = $"A new drive from {p.CompanyName} for {p.JobRole} requires your approval.",
            HtmlTemplateName = "DriveApprovalRequested",
            TemplateData = new Dictionary<string, string>
            {
                ["TpoName"] = p.TpoName,
                ["CollegeName"] = p.CollegeName,
                ["CompanyName"] = p.CompanyName,
                ["JobRole"] = p.JobRole,
                ["CTC"] = "As per company norms",
                ["Deadline"] = p.DriveDeadline,
                ["MinCgpa"] = "As specified",
                ["ActionUrl"] = $"/tpo/drives/{p.DriveId}/approve"
            },
            ActionUrl = $"/tpo/drives/{p.DriveId}/approve",
            ReferenceId = p.DriveId.ToString(),
            ReferenceType = "Drive",
            CorrelationId = envelope.CorrelationId,
            Channels = new() { NotificationChannel.Email, NotificationChannel.InApp }
        }, ct);
    }
}
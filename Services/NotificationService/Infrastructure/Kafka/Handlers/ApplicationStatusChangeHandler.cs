using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class ApplicationStatusChangeHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.ApplicationStatusChanged;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<ApplicationStatusChangeHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public ApplicationStatusChangeHandler(INotificationDispatcher dispatcher, ILogger<ApplicationStatusChangeHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<ApplicationStatusChangedEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        var templateData = new Dictionary<string, string>
        {
            ["Name"] = p.StudentName,
            ["CompanyName"] = p.CompanyName,
            ["JobRole"] = p.JobRole,
            ["DriveId"] = p.DriveId.ToString(),
            ["NewStatus"] = p.NewStatus.ToString(),
        };


        await _dispatcher.DispatchAsync(new DispatchNotificationRequest
        {
            RecipientUserId = p.StudentUserId,
            RecipientEmail = p.StudentEmail,
            RecipientName = p.StudentName,
            Type = NotificationType.ApplicationStatusChanged,
            Title = "Application Status Changed — PMS",
            Body = $"Hi {p.StudentName}, your application status for company {p.CompanyName} with job role {p.JobRole} has been updated to {p.NewStatus}.",
            HtmlTemplateName = "ApplicationStatusChanged",
            TemplateData = templateData,
            ActionUrl = null,
            CorrelationId = envelope.CorrelationId,
            Channels = new() { NotificationChannel.Email, NotificationChannel.InApp }  // Email and in-app — not just email
        }, ct);
    }
}
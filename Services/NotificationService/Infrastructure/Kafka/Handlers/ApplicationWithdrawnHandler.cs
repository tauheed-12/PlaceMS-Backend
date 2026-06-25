using System.Text.Json;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class ApplicationWithdrawnHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.ApplicationWithdrawn;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<ApplicationWithdrawnHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public ApplicationWithdrawnHandler(INotificationDispatcher dispatcher, ILogger<ApplicationWithdrawnHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<ApplicationWithdrawnEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        var templateData = new Dictionary<string, string>
        {
            ["Name"] = p.StudentName,
            ["CompanyName"] = p.CompanyName,
            ["JobRole"] = p.JobRole,
            ["DriveId"] = p.DriveId.ToString(),
            ["NewStatus"] = "Withdrawn",
        };


        await _dispatcher.DispatchAsync(new DispatchNotificationRequest
        {
            RecipientUserId = p.StudentUserId,
            RecipientEmail = p.StudentEmail,
            RecipientName = p.StudentName,
            Type = NotificationType.ApplicationWithdrawn,
            Title = "Application Withdrawn — PMS",
            Body = $"Hi {p.StudentName}, your application for company {p.CompanyName} with job role {p.JobRole} has been withdrawn.",
            HtmlTemplateName = "ApplicationWithdrawn",
            TemplateData = templateData,
            ActionUrl = null,
            CorrelationId = envelope.CorrelationId,
            Channels = new() { NotificationChannel.Email, NotificationChannel.InApp }  // Email and in-app — not just email
        }, ct);
    }
}
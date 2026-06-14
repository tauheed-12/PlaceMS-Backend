using System.Text.Json;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class ApplicationCreatedHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.ApplicationSubmitted;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<ApplicationCreatedHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public ApplicationCreatedHandler(INotificationDispatcher dispatcher, ILogger<ApplicationCreatedHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<ApplicationSubmittedEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        var templateData = new Dictionary<string, string>
        {
            ["Name"] = p.StudentName,
            ["CompanyName"] = p.CompanyName,
            ["JobRole"] = p.JobRole,
            ["DriveId"] = p.DriveId.ToString()
        };


        await _dispatcher.DispatchAsync(new DispatchNotificationRequest
        {
            RecipientUserId = p.StudentUserId,
            RecipientEmail = p.StudentEmail,
            RecipientName = p.StudentName,
            Type = NotificationType.ApplicationSubmitted,
            Title = "Application Submitted — PMS",
            Body = $"Hi {p.StudentName}, your application for company {p.CompanyName} with job role {p.JobRole} has been submitted successfully.",
            HtmlTemplateName = "ApplicationSubmitted",
            TemplateData = templateData,
            ActionUrl = null,
            CorrelationId = envelope.CorrelationId,
            Channels = new() { NotificationChannel.Email, NotificationChannel.InApp }  // Email and in-app — not just email
        }, ct);
    }
}
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace NotificationService.Infrastructure.Kafka.Handlers;

public class ApplicationStatusChangedHandler : INotificationEventHandler
{
    public string Topic => KafkaTopics.ApplicationStatusChanged;

    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<ApplicationStatusChangedHandler> _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public ApplicationStatusChangedHandler(
        INotificationDispatcher dispatcher,
        ILogger<ApplicationStatusChangedHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageValue, CancellationToken ct = default)
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<ApplicationStatusChangedEvent>>(messageValue, _json);
        if (envelope?.Payload is null) return;

        var p = envelope.Payload;

        var (type, title, template) = p.NewStatus switch
        {
            "UnderReview" => (NotificationType.ApplicationUnderReview, $"Your {p.CompanyName} application is under review", "ApplicationUnderReview"),
            "Shortlisted" => (NotificationType.ApplicationShortlisted, $"🎉 You've been shortlisted at {p.CompanyName}!", "ApplicationShortlisted"),
            "Offered" => (NotificationType.ApplicationOffered, $"🎊 Offer received from {p.CompanyName}!", "ApplicationOffered"),
            "Rejected" => (NotificationType.ApplicationRejected, $"Application update from {p.CompanyName}", "ApplicationRejected"),
            _ => (NotificationType.ApplicationSubmitted, $"Application status updated", "ApplicationStatusUpdated")
        };

        await _dispatcher.DispatchAsync(new DispatchNotificationRequest
        {
            RecipientUserId = p.StudentUserId,
            RecipientEmail = p.StudentEmail,
            RecipientName = p.StudentName,
            Type = type,
            Title = title,
            Body = $"Your application for {p.JobRole} at {p.CompanyName} has been updated to: {p.NewStatus}.",
            HtmlTemplateName = template,
            TemplateData = new Dictionary<string, string>
            {
                ["StudentName"] = p.StudentName,
                ["CompanyName"] = p.CompanyName,
                ["JobRole"] = p.JobRole,
                ["NewStatus"] = p.NewStatus,
                ["ActionUrl"] = "/student/applications"
            },
            ActionUrl = "/student/applications",
            ReferenceId = p.ApplicationId.ToString(),
            ReferenceType = "Application",
            CorrelationId = envelope.CorrelationId,
            Channels = new() { NotificationChannel.Email, NotificationChannel.InApp }
        }, ct);
    }
}
using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs.Requests;

public record UpdatePreferenceRequest
{
    public NotificationType NotificationType { get; init; }
    public bool EmailEnabled { get; init; }
    public bool InAppEnabled { get; init; }
}

// Internal request used by kafka handlers to dispatch notifications.
public record DispatchNotificationRequest
{
    public Guid RecipientUserId { get; init; }
    public string RecipientEmail { get; init; } = string.Empty;
    public string RecipientName { get; init; } = string.Empty;
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? HtmlTemplateName { get; init; }
    public Dictionary<string, string> TemplateData { get; init; } = new();
    public string? ActionUrl { get; init; }
    public string? ReferenceId { get; init; }
    public string? ReferenceType { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public List<NotificationChannel> Channels { get; init; } = new() { NotificationChannel.Email, NotificationChannel.InApp };
}

public record EmailMessage
{
    public string To { get; init; } = string.Empty;
    public string ToName { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string PlainBody { get; init; } = string.Empty;
    public string? HtmlBody { get; init; }
}

public record InAppNotificationPayload
{
    public Guid NotificationId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
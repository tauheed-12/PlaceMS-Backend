using NotificationService.Domain.Enums;
using SharedKernel.Abstractions;

namespace NotificationService.Domain.Entities;

/// <summary>
/// Represents a single notification record.
/// One record per channel per recipient per event.
/// e.g. DriveApproved fires 2 records: Email + InApp for the recruiter.
/// </summary>
public class Notification : BaseEntity
{
    // ── Recipient ─────────────────────────────────────────────────
    public Guid RecipientUserId { get; private set; }
    public string RecipientEmail { get; private set; } = string.Empty;
    public string RecipientName { get; private set; } = string.Empty;

    // ── Content ───────────────────────────────────────────────────
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;       // Plain text body
    public string? HtmlBody { get; private set; }                   // HTML for email
    public string? ActionUrl { get; private set; }                  // Deep link for in-app

    // ── Metadata ──────────────────────────────────────────────────
    public string? ReferenceId { get; private set; }                // DriveId, ApplicationId etc.
    public string? ReferenceType { get; private set; }              // "Drive", "Application" etc.
    public string CorrelationId { get; private set; } = string.Empty;

    // ── Delivery State ────────────────────────────────────────────
    public NotificationStatus Status { get; private set; } = NotificationStatus.Pending;
    public int RetryCount { get; private set; } = 0;
    public DateTime? SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? FailureReason { get; private set; }

    // EF Core
    private Notification() { }

    public static Notification Create(
        Guid recipientUserId,
        string recipientEmail,
        string recipientName,
        NotificationType type,
        NotificationChannel channel,
        string title,
        string body,
        string correlationId,
        string? htmlBody = null,
        string? actionUrl = null,
        string? referenceId = null,
        string? referenceType = null)
        => new()
        {
            RecipientUserId = recipientUserId,
            RecipientEmail = recipientEmail.ToLowerInvariant(),
            RecipientName = recipientName,
            Type = type,
            Channel = channel,
            Title = title,
            Body = body,
            HtmlBody = htmlBody,
            ActionUrl = actionUrl,
            CorrelationId = correlationId,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            Status = NotificationStatus.Pending
        };

    public void MarkSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void MarkFailed(string reason)
    {
        RetryCount++;
        FailureReason = reason;
        Status = RetryCount >= 3 ? NotificationStatus.Failed : NotificationStatus.Pending;
        SetUpdatedAt();
    }

    public void MarkRead()
    {
        if (Channel != NotificationChannel.InApp) return;
        Status = NotificationStatus.Read;
        ReadAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}

/// <summary>
/// Stores per-user notification preferences.
/// Users can opt out of specific notification types per channel.
/// Created with all defaults ON when user registers.
/// </summary>
public class NotificationPreference : BaseEntity
{
    public Guid UserId { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public bool EmailEnabled { get; private set; } = true;
    public bool InAppEnabled { get; private set; } = true;

    private NotificationPreference() { }

    public static NotificationPreference Create(Guid userId, NotificationType type)
        => new() { UserId = userId, NotificationType = type };

    /// <summary>
    /// Creates default preferences (all ON) for all notification types for a new user.
    /// </summary>
    public static List<NotificationPreference> CreateDefaults(Guid userId)
        => Enum.GetValues<NotificationType>()
               .Select(type => Create(userId, type))
               .ToList();

    public void UpdateEmail(bool enabled)
    {
        EmailEnabled = enabled;
        SetUpdatedAt();
    }

    public void UpdateInApp(bool enabled)
    {
        InAppEnabled = enabled;
        SetUpdatedAt();
    }
}
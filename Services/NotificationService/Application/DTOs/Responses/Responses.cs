using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs.Responses;

public record NotificationResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
}

public record NotificationPreferenceResponse
{
    public string NotificationType { get; init; } = string.Empty;
    public bool EmailEnabled { get; init; }
    public bool InAppEnabled { get; init; }
}

public record UnreadCountResponse
{
    public int Count { get; init; }
}
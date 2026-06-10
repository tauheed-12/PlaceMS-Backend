using Microsoft.AspNetCore.Mvc.RazorPages;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.DTOs.Responses;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using SharedKernel.Models;

namespace NotificationService.Application.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Notification>> GetByUserIdAsync(Guid userId, int page, int pageSize, bool? unreadOnly, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task<List<Notification>> GetPendingAsync(int batchSize, CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
    Task AddRangeAsync(List<Notification> notifications, CancellationToken ct = default);
    void Update(Notification notification);
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface INotificationPreferenceRepository
{
    Task<List<NotificationPreference>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<NotificationPreference?> GetAsync(Guid userId, NotificationType type, CancellationToken ct = default);
    Task AddRangeAsync(List<NotificationPreference> preferences, CancellationToken ct = default);
    void Update(NotificationPreference preference);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}

public interface IInAppNotificationService
{
    /// <summary>Pushes real-time notification to connected user via SignalR.</summary>
    Task PushAsync(Guid userId, InAppNotificationPayload payload, CancellationToken ct = default);
}

public interface ITemplateEngine
{
    /// <summary>
    /// Renders an HTML email template by name with provided data.
    /// Templates live in Application/Templates/*.html
    /// </summary>
    string Render(string templateName, Dictionary<string, string> data);
}

public interface INotificationService
{
    Task<PagedResult<NotificationResponse>> GetMyNotificationsAsync(Guid userId, int page, int pageSize, bool? unreadOnly, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
    Task<List<NotificationPreferenceResponse>> GetPreferencesAsync(Guid userId, CancellationToken ct = default);
    Task UpdatePreferenceAsync(Guid userId, UpdatePreferenceRequest request, CancellationToken ct = default);
}

public interface INotificationDispatcher
{
    /// <summary>
    /// Core dispatch method. Creates Notification records and routes
    /// to email sender and/or SignalR hub based on channel + user preferences.
    /// </summary>
    Task DispatchAsync(DispatchNotificationRequest request, CancellationToken ct = default);
}
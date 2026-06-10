using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.DTOs.Responses;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using SharedKernel.Exceptions;
using SharedKernel.Models;

namespace NotificationService.Application.Services;

public class NotificationAppService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly INotificationPreferenceRepository _prefRepo;
    private readonly ILogger<NotificationAppService> _logger;

    public NotificationAppService(
        INotificationRepository repo,
        INotificationPreferenceRepository prefRepo,
        ILogger<NotificationAppService> logger)
    {
        _repo = repo;
        _prefRepo = prefRepo;
        _logger = logger;
    }

    public async Task<PagedResult<NotificationResponse>> GetMyNotificationsAsync(
        Guid userId, int page, int pageSize, bool? unreadOnly, CancellationToken ct = default)
    {
        var paged = await _repo.GetByUserIdAsync(userId, page, pageSize, unreadOnly, ct);
        var items = paged.Items.Select(n => new NotificationResponse
        {
            Id = n.Id,
            Title = n.Title,
            Body = n.Body,
            ActionUrl = n.ActionUrl,
            Type = n.Type.ToString(),
            Status = n.Status.ToString(),
            IsRead = n.Status == Domain.Enums.NotificationStatus.Read,
            CreatedAt = n.CreatedAt,
            ReadAt = n.ReadAt
        }).ToList();

        return PagedResult<NotificationResponse>.Create(items, paged.TotalCount, page, pageSize);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
        => await _repo.GetUnreadCountAsync(userId, ct);

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _repo.GetByIdAsync(notificationId, ct)
            ?? throw new NotFoundException("Notification", notificationId);

        if (notification.RecipientUserId != userId)
            throw new ForbiddenException("You can only mark your own notifications as read.");

        notification.MarkRead();
        _repo.Update(notification);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        await _repo.MarkAllReadAsync(userId, ct);
        _logger.LogInformation("Marked all notifications read for user {UserId}", userId);
    }

    public async Task<List<NotificationPreferenceResponse>> GetPreferencesAsync(
        Guid userId, CancellationToken ct = default)
    {
        var prefs = await _prefRepo.GetByUserIdAsync(userId, ct);
        return prefs.Select(p => new NotificationPreferenceResponse
        {
            NotificationType = p.NotificationType.ToString(),
            EmailEnabled = p.EmailEnabled,
            InAppEnabled = p.InAppEnabled
        }).ToList();
    }

    public async Task UpdatePreferenceAsync(Guid userId, UpdatePreferenceRequest request,
        CancellationToken ct = default)
    {
        var pref = await _prefRepo.GetAsync(userId, request.NotificationType, ct);

        if (pref is null)
        {
            // Create if doesn't exist yet
            var newPref = NotificationPreference.Create(userId, request.NotificationType);
            newPref.UpdateEmail(request.EmailEnabled);
            newPref.UpdateInApp(request.InAppEnabled);
            await _prefRepo.AddRangeAsync(new List<NotificationPreference> { newPref }, ct);
        }
        else
        {
            pref.UpdateEmail(request.EmailEnabled);
            pref.UpdateInApp(request.InAppEnabled);
            _prefRepo.Update(pref);
        }

        await _prefRepo.SaveChangesAsync(ct);
    }
}
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Persistence;
using SharedKernel.Models;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
        => _context = context;

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task<PagedResult<Notification>> GetByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        bool? unreadOnly,
        CancellationToken ct = default)
    {
        var query = _context.Notifications
            .Where(n => n.RecipientUserId == userId
                     && n.Channel == NotificationChannel.InApp);

        if (unreadOnly == true)
            query = query.Where(n => n.Status != NotificationStatus.Read);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedResult<Notification>.Create(items, total, page, pageSize);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
        => await _context.Notifications
            .CountAsync(n =>
                n.RecipientUserId == userId &&
                n.Channel == NotificationChannel.InApp &&
                n.Status != NotificationStatus.Read, ct);

    public async Task<List<Notification>> GetPendingAsync(int batchSize, CancellationToken ct = default)
        => await _context.Notifications
            .Where(n => n.Status == NotificationStatus.Pending && n.RetryCount < 3)
            .OrderBy(n => n.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
        => await _context.Notifications.AddAsync(notification, ct);

    public async Task AddRangeAsync(List<Notification> notifications, CancellationToken ct = default)
        => await _context.Notifications.AddRangeAsync(notifications, ct);

    public void Update(Notification notification)
        => _context.Notifications.Update(notification);

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
        => await _context.Notifications
            .Where(n =>
                n.RecipientUserId == userId &&
                n.Channel == NotificationChannel.InApp &&
                n.Status != NotificationStatus.Read)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Status, NotificationStatus.Read)
                .SetProperty(n => n.ReadAt, DateTime.UtcNow)
                .SetProperty(n => n.UpdatedAt, DateTime.UtcNow),
            ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
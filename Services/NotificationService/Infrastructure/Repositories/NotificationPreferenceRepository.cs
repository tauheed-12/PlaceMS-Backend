using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationPreferenceRepository : INotificationPreferenceRepository
{
    private readonly NotificationDbContext _context;

    public NotificationPreferenceRepository(NotificationDbContext context)
        => _context = context;

    public async Task<List<NotificationPreference>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default)
        => await _context.NotificationPreferences
            .Where(p => p.UserId == userId)
            .ToListAsync(ct);

    public async Task<NotificationPreference?> GetAsync(
        Guid userId,
        NotificationType type,
        CancellationToken ct = default)
        => await _context.NotificationPreferences
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.NotificationType == type, ct);

    public async Task AddRangeAsync(
        List<NotificationPreference> preferences,
        CancellationToken ct = default)
        => await _context.NotificationPreferences.AddRangeAsync(preferences, ct);

    public void Update(NotificationPreference preference)
        => _context.NotificationPreferences.Update(preference);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
using ApplicationService.Application.Interfaces;
using ApplicationService.Domain.Entities;
using ApplicationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using SharedKernel.Models;

namespace ApplicationService.Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddApplicationAsync(StudentApplication application, CancellationToken ct = default)
    {
        await _context.Applications.AddAsync(application, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<StudentApplication?> GetApplicationByIdAsync(Guid applicationId, CancellationToken ct = default)
        => await _context.Applications.FindAsync(new object[] { applicationId }, ct);

    public async Task<StudentApplication?> GetApplicationByStudentAndDriveAsync(Guid studentId, Guid driveId, CancellationToken ct = default)
        => await _context.Applications
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.DriveId == driveId, ct);

    public async Task<PagedResult<StudentApplication>> GetApplicationsByDriveIdAsync(Guid driveId, int pageNumber, int pageSize, string? search = null, ApplicationStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.Applications.Where(a => a.DriveId == driveId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.StudentName.Contains(search) || a.StudentEmail.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(a => a.AppliedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedResult<StudentApplication>.Create(items, total, pageNumber, pageSize);
    }

    public async Task<PagedResult<StudentApplication>> GetApplicationsByStudentIdAsync(Guid studentId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Applications.Where(a => a.StudentId == studentId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(a => a.AppliedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedResult<StudentApplication>.Create(items, total, pageNumber, pageSize);
    }

    public async Task<PagedResult<StudentApplication>> GetApplicationsByCollegeIdAsync(Guid collegeId, int pageNumber, int pageSize, string? search = null, ApplicationStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.Applications.Where(a => a.CollegeId == collegeId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.StudentName.Contains(search) || a.StudentEmail.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(a => a.AppliedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedResult<StudentApplication>.Create(items, total, pageNumber, pageSize);
    }

    public async Task UpdateApplicationAsync(StudentApplication application, CancellationToken ct = default)
    {
        _context.Applications.Update(application);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteApplicationAsync(Guid applicationId, CancellationToken ct = default)
    {
        var application = await GetApplicationByIdAsync(applicationId, ct);
        if (application is null)
            return;

        _context.Applications.Remove(application);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid applicationId, CancellationToken ct = default)
        => await _context.Applications.AnyAsync(a => a.Id == applicationId, ct);

    public async Task<bool> HasAppliedAsync(Guid studentId, Guid driveId, CancellationToken ct = default)
        => await _context.Applications.AnyAsync(a => a.StudentId == studentId && a.DriveId == driveId, ct);

    public async Task<Dictionary<ApplicationStatus, int>> GetStatusCountsForDriveAsync(Guid driveId, CancellationToken ct = default)
        => await GetStatusCountsAsync(_context.Applications.Where(a => a.DriveId == driveId), ct);

    public async Task<Dictionary<ApplicationStatus, int>> GetStatusCountsForCollegeAsync(Guid collegeId, CancellationToken ct = default)
        => await GetStatusCountsAsync(_context.Applications.Where(a => a.CollegeId == collegeId), ct);

    public async Task<Dictionary<ApplicationStatus, int>> GetStatusCountsForStudentAsync(Guid studentId, CancellationToken ct = default)
        => await GetStatusCountsAsync(_context.Applications.Where(a => a.StudentId == studentId), ct);

    private static async Task<Dictionary<ApplicationStatus, int>> GetStatusCountsAsync(IQueryable<StudentApplication> query, CancellationToken ct)
    {
        var result = await query
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(k => k.Status, v => v.Count, ct);

        return Enum.GetValues<ApplicationStatus>().ToDictionary(status => status, status => result.TryGetValue(status, out var count) ? count : 0);
    }
}

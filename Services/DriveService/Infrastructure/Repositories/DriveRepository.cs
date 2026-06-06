using DriveService.Application.Interfaces;
using DriveService.Domain.Entities;
using DriveService.Infrastructure.Persistence;
using SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Models;

namespace DriveService.Infrastructure.Repositories;

public class DriveRepository : IDriveRepository
{
    private readonly DriveDbContext _dbContext;
    private readonly ILogger<DriveRepository> _logger;

    public DriveRepository(DriveDbContext dbContext, ILogger<DriveRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // Basic Lookups
    public async Task<Drive?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Drives.FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<Drive?> GetByIdWithCollegesAsync(Guid id, CancellationToken ct)
    {
        return await _dbContext.Drives
            .Include(d => d.DriveColleges)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<Drive?> GetByIdWithAllAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Drives
            .Include(d => d.DriveColleges)
            .Include(d => d.DriveRounds)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    // Recruiter Queries
    public async Task<PagedResult<Drive>> GetByRecruiterAsync(
        Guid recruiterUserId,
        int page,
        int pageSize,
        string? search,
        bool? isDeactivated,
        CancellationToken ct = default
    )
    {
        var query = _dbContext.Drives
            .Where(d => d.RecruiterUserId == recruiterUserId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d =>
                d.JobRole.ToLower().Contains(search.ToLower()) ||
                d.Location.ToLower().Contains(search.ToLower()));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedResult<Drive>.Create(items, total, page, pageSize);
    }

    // TPO Queries
    public async Task<PagedResult<Drive>> GetByCollegeAsync(
        Guid collegeId,
        int page,
        int pageSize,
        string? search,
        CancellationToken ct = default
    )
    {
        var query = _dbContext.Drives
            .Where(d => d.DriveColleges.Any(dc => dc.CollegeId == collegeId))
            .Where(d => !d.IsDeactivated);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d =>
                d.CompanyName.ToLower().Contains(search.ToLower()) ||
                d.JobRole.ToLower().Contains(search.ToLower()));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(d => d.DriveColleges.Where(dc => dc.CollegeId == collegeId))
            .Include(d => d.DriveRounds)
            .ToListAsync(ct);

        return PagedResult<Drive>.Create(items, total, page, pageSize);
    }

    public async Task<PagedResult<Drive>> GetPendingByCollegeAsync(
        Guid collegeId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbContext.Drives
            .Where(d => d.DriveColleges.Any(dc =>
                dc.CollegeId == collegeId &&
                dc.ApprovalStatus == DriveApprovalStatus.Pending))
            .Where(d => !d.IsDeactivated);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(d => d.ApplicationDeadline)   // Most urgent first
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(d => d.DriveColleges.Where(dc => dc.CollegeId == collegeId))
            .Include(d => d.DriveRounds)
            .ToListAsync(ct);

        return PagedResult<Drive>.Create(items, total, page, pageSize);
    }

    // Student Queries 
    public async Task<PagedResult<Drive>> GetAvailableForCollegeAsync(
        Guid collegeId,
        int page,
        int pageSize,
        string? search,
        string? employmentType,
        CancellationToken ct = default)
    {
        var query = _dbContext.Drives
            .Where(d =>
                d.DriveColleges.Any(dc =>
                    dc.CollegeId == collegeId &&
                    dc.ApprovalStatus == DriveApprovalStatus.Approved) &&
                !d.IsDeactivated &&
                d.ApplicationDeadline >= DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d =>
                d.CompanyName.ToLower().Contains(search.ToLower()) ||
                d.JobRole.ToLower().Contains(search.ToLower()) ||
                d.Location.ToLower().Contains(search.ToLower()));

        if (!string.IsNullOrWhiteSpace(employmentType) &&
            Enum.TryParse<EmploymentType>(employmentType, out var empType))
            query = query.Where(d => d.EmploymentType == empType);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(d => d.ApplicationDeadline)   // Nearest deadline first
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(d => d.DriveRounds)
            .ToListAsync(ct);

        return PagedResult<Drive>.Create(items, total, page, pageSize);
    }

    // Admin Queries 
    public async Task<PagedResult<Drive>> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        Guid? collegeId,
        Guid? recruiterUserId,
        bool? isDeactivated,
        CancellationToken ct = default)
    {
        var query = _dbContext.Drives.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d =>
                d.CompanyName.ToLower().Contains(search.ToLower()) ||
                d.JobRole.ToLower().Contains(search.ToLower()));

        if (collegeId.HasValue)
            query = query.Where(d =>
                d.DriveColleges.Any(dc => dc.CollegeId == collegeId.Value));

        if (recruiterUserId.HasValue)
            query = query.Where(d => d.RecruiterUserId == recruiterUserId.Value);

        if (isDeactivated.HasValue)
            query = query.Where(d => d.IsDeactivated == isDeactivated.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(d => d.DriveColleges)
            .ToListAsync(ct);

        return PagedResult<Drive>.Create(items, total, page, pageSize);
    }

    // Internal Queries 
    public async Task<DriveCollege?> GetDriveCollegeAsync(
        Guid driveId,
        Guid collegeId,
        CancellationToken ct = default)
        => await _dbContext.DriveColleges
            .FirstOrDefaultAsync(dc =>
                dc.DriveId == driveId &&
                dc.CollegeId == collegeId, ct);

    // Write
    public async Task AddAsync(Drive drive, CancellationToken ct = default)
        => await _dbContext.Drives.AddAsync(drive, ct);

    public void Update(Drive drive)
        => _dbContext.Drives.Update(drive);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _dbContext.SaveChangesAsync(ct);
}
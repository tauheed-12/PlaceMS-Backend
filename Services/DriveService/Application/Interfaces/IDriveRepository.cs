// Application/Interfaces/IDriveRepository.cs
using DriveService.Domain.Entities;
using SharedKernel.Models;

namespace DriveService.Application.Interfaces;

public interface IDriveRepository
{
    // ── Basic Lookups ─────────────────────────────────────────────
    Task<Drive?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Drive?> GetByIdWithCollegesAsync(Guid id, CancellationToken ct = default);
    Task<Drive?> GetByIdWithAllAsync(Guid id, CancellationToken ct = default);

    // ── Recruiter Queries ─────────────────────────────────────────
    Task<PagedResult<Drive>> GetByRecruiterAsync(
        Guid recruiterUserId,
        int page,
        int pageSize,
        string? search,
        bool? isDeactivated,
        CancellationToken ct = default);

    // ── TPO Queries ───────────────────────────────────────────────
    Task<PagedResult<Drive>> GetByCollegeAsync(
        Guid collegeId,
        int page,
        int pageSize,
        string? search,
        CancellationToken ct = default);

    Task<PagedResult<Drive>> GetPendingByCollegeAsync(
        Guid collegeId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    // ── Student Queries ───────────────────────────────────────────
    /// <summary>
    /// Returns drives approved for the given college,
    /// not deactivated, and deadline not passed.
    /// </summary>
    Task<PagedResult<Drive>> GetAvailableForCollegeAsync(
        Guid collegeId,
        int page,
        int pageSize,
        string? search,
        string? employmentType,
        CancellationToken ct = default);

    // ── Admin Queries ─────────────────────────────────────────────
    Task<PagedResult<Drive>> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        Guid? collegeId,
        Guid? recruiterUserId,
        bool? isDeactivated,
        CancellationToken ct = default);

    // ── Internal Queries ──────────────────────────────────────────
    Task<DriveCollege?> GetDriveCollegeAsync(
        Guid driveId,
        Guid collegeId,
        CancellationToken ct = default);

    // ── Write ─────────────────────────────────────────────────────
    Task AddAsync(Drive drive, CancellationToken ct = default);
    void Update(Drive drive);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
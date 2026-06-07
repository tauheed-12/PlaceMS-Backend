using ApplicationService.Domain.Entities;
using SharedKernel.Enums;
using SharedKernel.Models;

namespace ApplicationService.Application.Interfaces;

public interface IApplicationRepository
{
    Task AddApplicationAsync(StudentApplication application, CancellationToken ct = default);
    Task<StudentApplication?> GetApplicationByIdAsync(Guid applicationId, CancellationToken ct = default);
    Task<StudentApplication?> GetApplicationByStudentAndDriveAsync(Guid studentId, Guid driveId, CancellationToken ct = default);
    Task<PagedResult<StudentApplication>> GetApplicationsByDriveIdAsync(Guid driveId, int pageNumber, int pageSize, string? search = null, ApplicationStatus? status = null, CancellationToken ct = default);
    Task<PagedResult<StudentApplication>> GetApplicationsByStudentIdAsync(Guid studentId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<PagedResult<StudentApplication>> GetApplicationsByCollegeIdAsync(Guid collegeId, int pageNumber, int pageSize, string? search = null, ApplicationStatus? status = null, CancellationToken ct = default);
    Task UpdateApplicationAsync(StudentApplication application, CancellationToken ct = default);
    Task DeleteApplicationAsync(Guid applicationId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid applicationId, CancellationToken ct = default);
    Task<bool> HasAppliedAsync(Guid studentId, Guid driveId, CancellationToken ct = default);
    Task<Dictionary<ApplicationStatus, int>> GetStatusCountsForDriveAsync(Guid driveId, CancellationToken ct = default);
    Task<Dictionary<ApplicationStatus, int>> GetStatusCountsForCollegeAsync(Guid collegeId, CancellationToken ct = default);
    Task<Dictionary<ApplicationStatus, int>> GetStatusCountsForStudentAsync(Guid studentId, CancellationToken ct = default);
}

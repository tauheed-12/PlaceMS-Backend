using SharedKernel.Models;
using ApplicationService.Application.DTOs.Requests;
using ApplicationService.Application.DTOs.Responses;

namespace ApplicationService.Application.Interfaces;

public interface IApplicationService
{
    Task<CreateApplicationResponseDto> ApplyAsync(Guid studentId, CreateApplicationRequestDto request, CancellationToken ct = default);
    Task<WithdrawApplicationResponseDto> WithdrawAsync(Guid applicationId, Guid studentId, CancellationToken ct = default);
    Task<UpdateApplicationStatusResponseDto> UpdateStatusAsync(Guid applicationId, UpdateApplicationStatusRequestDto request, CancellationToken ct = default);

    Task<PagedResult<StudentApplicationDto>> GetByStudentIdAsync(Guid studentId, ApplicationFilterRequestDto request, CancellationToken ct = default);
    Task<PagedResult<ApplicationDetailsDto>> GetByDriveIdAsync(Guid driveId, ApplicationFilterRequestDto request, CancellationToken ct = default);
    Task<PagedResult<ApplicationDetailsDto>> GetByCollegeIdAsync(Guid collegeId, ApplicationFilterRequestDto request, CancellationToken ct = default);
    Task<ApplicationShortDto?> GetByStudentAndDriveAsync(Guid studentId, Guid driveId, CancellationToken ct = default);

    Task<ApplicationStatisticsDto> GetDriveStatisticsAsync(Guid driveId, CancellationToken ct = default);
    Task<ApplicationStatisticsDto> GetCollegeStatisticsAsync(Guid collegeId, CancellationToken ct = default);
    Task<ApplicationStatisticsDto> GetStudentStatisticsAsync(Guid studentId, CancellationToken ct = default);

    Task<bool> HasAppliedAsync(Guid studentId, Guid driveId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid applicationId, CancellationToken ct = default);
    Task<bool> IsOwnerAsync(Guid applicationId, Guid studentId, CancellationToken ct = default);

    Task BulkShortlistAsync();
    Task BulkRejectAsync();
    Task BulkUpdateStatusAsync();
    Task ExportApplicationsAsync();
}

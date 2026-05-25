using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;

namespace CollegeService.Application.Interfaces.Services;

public interface IAdminCollegeScopeService
{
    Task<AdminCollegeScopeResponseDto> AssignCollegeToAdminAsync(Guid adminUserId, Guid collegeId, CancellationToken ct);
    Task RemoveCollegeFromAdminAsync(Guid adminUserId, Guid collegeId, CancellationToken ct);
    Task<bool> HasAccessToCollegeAsync(Guid adminUserId, Guid collegeId, CancellationToken ct);
    Task<List<Guid>> GetCollegesIdsByAdminIdAsync(Guid adminUserId, CancellationToken ct);
    Task<PaginatedResponseDto<CollegeShortDto>> GetCollegesByAdminIdAsync(Guid adminId, int pageNumber, int pageSize, CancellationToken ct);
    Task<PaginatedResponseDto<TpoDetailsDto>> GetTposByAdminIdAsync(Guid adminUserId, int pageNumber, int pageSize, CancellationToken ct);
}
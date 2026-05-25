using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;

public interface IAdminCollegeScopeService
{
    Task<AdminCollegeScopeResponseDto> AssignCollegeToAdminAsync(Guid adminUserId, Guid collegeId, CancellationToken ct);
    Task RemoveCollegeFromAdminAsync(Guid adminUserId, Guid collegeId, CancellationToken ct);
    Task<bool> HasAccessToCollegeAsync(Guid adminUserId, Guid collegeId, CancellationToken ct);
    Task<List<Guid>> GetCollegesByAdminIdAsync(Guid adminUserId, CancellationToken ct);
    Task<List<TpoDetailsDto>> GetTposByAdminIdAsync(Guid adminUserId, CancellationToken ct);
}
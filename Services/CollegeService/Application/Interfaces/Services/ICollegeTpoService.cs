using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;
using SharedKernel.Enums;

namespace CollegeService.Application.Interfaces.Services;

public interface ICollegeTpoService
{
    Task<TpoDetailsDto> AssignPrimaryTpoAsync(CreateTpoRequestDto request, Guid assignedBy, CancellationToken ct);
    Task RemoveTpoAsync(Guid collegeId, Guid userId, CancellationToken ct);
    // Task<List<TpoShortDto>> GetTposByCollegeIdAsync(Guid collegeId, CancellationToken ct);
    Task<TpoDetailsDto?> GetPrimaryTpoByCollegeIdAsync(Guid collegeId, CancellationToken ct);
    Task<bool> IsPrimaryTpoAsync(Guid collegeId, Guid userId, CancellationToken ct);
}
